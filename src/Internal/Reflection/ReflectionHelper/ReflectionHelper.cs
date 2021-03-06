using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CloudState.CSharpSupport.Attributes;
using CloudState.CSharpSupport.Exceptions;
using CloudState.CSharpSupport.Interfaces.Contexts;
using CloudState.CSharpSupport.Interfaces.Services;

namespace CloudState.CSharpSupport.Reflection.ReflectionHelper
{
    internal partial class ReflectionHelper
    {
        private static Func<Type, IList<MethodInfo>> DeclaredMethodDiscovery { get; set; } = GetAllDeclaredMethods;

        internal static void RegisterDeclaredMethodDiscovery(Func<Type, IList<MethodInfo>> discoveryOverride)
        {
            DeclaredMethodDiscovery = discoveryOverride;
        }

        internal static IList<MethodInfo> GetAllDeclaredMethods(Type type)
        {
            if (type.BaseType == null || type.BaseType == typeof(object))
            {
                return type.GetMethods().ToList();
            }
            else
            {
                return type.GetMethods(BindingFlags.DeclaredOnly)
                    .Concat(GetAllDeclaredMethods(type.BaseType))
                    .ToList();
            }
        }

        internal static string GetCapitalizedName(MethodInfo method)
        {
            if (char.IsLower(method.Name[0]))
            {
                return $"{char.ToUpper(method.Name[0])}{method.Name.Substring(1)}";
            }
            else
            {
                return method.Name;
            }
        }

        internal static bool IsWithinBounds(Type clazz, Type upper, Type lower)
        {
            return upper.IsAssignableFrom(clazz) && clazz.IsAssignableFrom(lower);
        }

        /*
        // NOTE: This isn't applicable to C# - can't change accessibility
        def ensureAccessible[T <: AccessibleObject](accessible: T): T = {
            if (!accessible.isAccessible) {
            accessible.setAccessible(true)
            }
            accessible
        } */


        internal static ParameterHandler<TContext>[] GetParameterHandlers<TContext>(MethodBase method)
            where TContext : IContext
        {
            var methodParameters = method.GetParameters().ToArray();
            var handlers = new ParameterHandler<TContext>[methodParameters.Length];
            for (var i = 0; i < methodParameters.Length; i++)
            {
                var parameter = new MethodParameter(method, i);
                var contextClass = typeof(TContext);
                if (IsWithinBounds(parameter.ParameterType, typeof(IContext), contextClass))
                {
                    handlers[i] = new ContextParameterHandler<TContext>();
                }
                else if (typeof(IContext).IsAssignableFrom(parameter.ParameterType))
                {
                    throw new CloudStateException(
                        $"Unsupported context parameter on [{method.Name}], " +
                        $"[{parameter.ParameterType.Name}] must be the same or a super type of [{contextClass.Name}]"
                    );
                }
                else if (parameter.ParameterType == typeof(IServiceCallFactory))
                {
                    handlers[i] = new ServiceCallFactoryParameterHandler<TContext>();
                }
                else if (parameter.Attributes.Any(x => typeof(EntityIdAttribute) == x.GetType()))
                {
                    if (parameter.ParameterType != typeof(string))
                    {
                        throw new CloudStateException(
                            $"[EntityIdAttribute] annotated parameter on method {method.Name} " +
                            $"has type {parameter.ParameterType}, but must be String."
                        );
                    }

                    handlers[i] = new EntityIdParameterHandler<TContext>();
                }
                else
                {
                    // TODO: revisit extra arguments implementation
                    handlers[i] = new MainArgumentParameterHandler<TContext>(parameter.ParameterType);
                }
            }

            return handlers;

        }

        /*
            def validateNoBadMethods(methods: Seq[Method],
                                    entity: Class[_ <: Annotation],
                                    allowed: Set[Class[_ <: Annotation]]): Unit =
                methods.foreach { method =>
                method.getAnnotations.foreach { annotation =>
                    if (annotation.annotationType().getAnnotation(classOf[CloudStateAnnotation]) != null && !allowed(
                        annotation.annotationType()
                        )) {
                    val maybeAlternative = allowed.find(_.getSimpleName == annotation.annotationType().getSimpleName)
                    throw new RuntimeException(
                        s"Annotation @${annotation.annotationType().getName} on method ${method.getDeclaringClass.getName}." +
                        s"${method.getName} not allowed in @${entity.getName} annotated entity." +
                        maybeAlternative.fold("")(alterative => s" Did you mean to use @${alterative.getName}?")
                    )
                    }
                }
            }
        */

    }
}