using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Unity.Mvc3;
using System.Web.Http;
using Microsoft.Practices.ServiceLocation;
using Twitter_Shared.Data;
using System.Data.Entity;

namespace Twitter.Application
{
    public static class Bootstrapper
    {
        public static void Initialise()
        {
            var container = BuildUnityContainer();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
            GlobalConfiguration.Configuration.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);
            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(container));
        }

        private static IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();
            container.RegisterType(typeof(IUnitOfWork), typeof(UnitOfWork));
            container.RegisterType(typeof(IEntityRepository<>), typeof(EntityRepository<>));
            container.RegisterType(typeof(DbContext), typeof(TwitterContext));

            container.RegisterInstance(new TwitterContextAdapter(container.Resolve<DbContext>()), new HierarchicalLifetimeManager());
            container.RegisterType<IDbSetFactory>(new InjectionFactory(con => con.Resolve<TwitterContextAdapter>()));
            container.RegisterType<IDbContext>(new InjectionFactory(con => con.Resolve<TwitterContextAdapter>()));

            return container;
        }
    }
}