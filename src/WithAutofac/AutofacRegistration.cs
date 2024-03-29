using System;
using System.Reflection;
using Autofac.Builder;
using Microsoft.Extensions.DependencyInjection;
using Autofac;
using Common;

namespace WithAutofac
{
    /// <summary>
    /// Extension methods for registering ASP.NET Core dependencies with Autofac.
    /// </summary>
    public static class AutofacRegistration
    {
        /// <summary>
        /// Populates the Autofac container builder with the set of registered service descriptors
        /// and makes <see cref="IServiceProvider"/> and <see cref="IServiceScopeFactory"/>
        /// available in the container.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="ContainerBuilder"/> into which the registrations should be made.
        /// </param>
        /// <param name="services">
        /// The set of service descriptors to register in the container.
        /// </param>
        public static void Populate(
                this ContainerBuilder builder,
                IServiceCollection services)
        {
            builder.RegisterType<AutofacServiceProvider>().As<IServiceProvider>();
            builder.RegisterType<AutofacServiceScopeFactory>().As<IServiceScopeFactory>();

            Register(builder, services);
        }

        /// <summary>
        /// Configures the lifecycle on a service registration.
        /// </summary>
        /// <typeparam name="TActivatorData">The activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">The object registration style.</typeparam>
        /// <param name="registrationBuilder">The registration being built.</param>
        /// <param name="lifecycleKind">The lifecycle specified on the service registration.</param>
        /// <returns>
        /// The <paramref name="registrationBuilder" />, configured with the proper lifetime scope,
        /// and available for additional configuration.
        /// </returns>
        private static IRegistrationBuilder<object, TActivatorData, TRegistrationStyle> ConfigureLifecycle<TActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<object, TActivatorData, TRegistrationStyle> registrationBuilder,
                ServiceLifetime lifecycleKind)
        {
            switch (lifecycleKind)
            {
                case ServiceLifetime.Singleton:
                    registrationBuilder.SingleInstance();
                    break;
                case ServiceLifetime.Scoped:
                    registrationBuilder.InstancePerLifetimeScope();
                    break;
                case ServiceLifetime.Transient:
                    registrationBuilder.InstancePerDependency();
                    break;
            }

            return registrationBuilder;
        }

        /// <summary>
        /// Populates the Autofac container builder with the set of registered service descriptors.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="ContainerBuilder"/> into which the registrations should be made.
        /// </param>
        /// <param name="services">
        /// The set of service descriptors to register in the container.
        /// </param>
        private static void Register(
                ContainerBuilder builder,
                IServiceCollection services)
        {
            // TODO:yh 此处注释掉部分与abp相关的东西

            // yh 此处主要作用是获取所有的 AbpModule 的类型,启用属性注入 到所有属于  xxxAbpModule 程序集的类型
            //var moduleContainer = services.GetSingletonInstance<IModuleContainer>();

            // yh 拦截器
            //var registrationActionList = services.GetRegistrationActionList();

            foreach (var service in services)
            {
                if (service.ImplementationType != null)
                {
                    // Test if the an open generic type is being registered
                    var serviceTypeInfo = service.ServiceType.GetTypeInfo();
                    if (serviceTypeInfo.IsGenericTypeDefinition)
                    {
                        builder
                            .RegisterGeneric(service.ImplementationType)
                            .As(service.ServiceType)
                            .ConfigureLifecycle(service.Lifetime);

                        // yh 处理启用 属性注入 和 拦截器
                        //.ConfigureAbpConventions(moduleContainer, registrationActionList);
                    }
                    else
                    {
                        builder
                            .RegisterType(service.ImplementationType)
                            .As(service.ServiceType)
                            .ConfigureLifecycle(service.Lifetime);

                        // yh 处理启用 属性注入 和 拦截器
                        //.ConfigureAbpConventions(moduleContainer, registrationActionList);
                    }
                }
                else if (service.ImplementationFactory != null)
                {
                    var registration = RegistrationBuilder.ForDelegate(service.ServiceType, (context, parameters) =>
                    {
                        var serviceProvider = context.Resolve<IServiceProvider>();
                        return service.ImplementationFactory(serviceProvider);
                    })
                    .ConfigureLifecycle(service.Lifetime)
                    .CreateRegistration();
                    //TODO: ConfigureAbpConventions ?

                    builder.RegisterComponent(registration);
                }
                else
                {
                    builder
                        .RegisterInstance(service.ImplementationInstance)
                        .As(service.ServiceType)
                        .ConfigureLifecycle(service.Lifetime);
                }
            }
        }
    }
}