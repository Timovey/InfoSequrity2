using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Unity.Lifetime;
using Unity;

namespace InfoSequrity2
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IUnityContainer currentContainer = BuildUnityContainer();

            var authWindow = currentContainer.Resolve<MainWindow>();
            authWindow.Show();
        }


        private static IUnityContainer BuildUnityContainer()
        {
            var currentContainer = new UnityContainer();
            currentContainer.RegisterType<CryptoLogic>(new HierarchicalLifetimeManager());
            return currentContainer;
        }
    }
}
