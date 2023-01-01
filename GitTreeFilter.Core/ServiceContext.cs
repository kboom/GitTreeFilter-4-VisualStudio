using System;
using Microsoft.Extensions.DependencyInjection;

namespace GitTreeFilter.Core
{
    internal static class ServiceContext
    {
        private static IServiceProvider instance;

        public static IServiceProvider Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = BuildProvider();
                }

                return instance;
            }
        }

        private static IServiceProvider BuildProvider()
        {
            var container = new ServiceCollection();
            container.AddScoped<GitRepositoryFactory>();
            return container.BuildServiceProvider();
        }
    }
}
