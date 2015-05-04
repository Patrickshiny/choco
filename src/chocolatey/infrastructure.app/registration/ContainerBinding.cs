﻿// Copyright © 2011 - Present RealDimensions Software, LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// 
// You may obtain a copy of the License at
// 
// 	http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace chocolatey.infrastructure.app.registration
{
    using System.Collections.Generic;
    using NuGet;
    using SimpleInjector;
    using adapters;
    using commands;
    using cryptography;
    using events;
    using filesystem;
    using infrastructure.commands;
    using infrastructure.configuration;
    using infrastructure.services;
    using nuget;
    using services;
    using IFileSystem = filesystem.IFileSystem;
    using IHashProvider = cryptography.IHashProvider;

    // ReSharper disable InconsistentNaming

    /// <summary>
    ///   The main inversion container registration for the application. Look for other container bindings in client projects.
    /// </summary>
    public sealed class ContainerBinding
    {
        /// <summary>
        ///   Loads the module into the kernel.
        /// </summary>
        public void RegisterComponents(Container container)
        {
            var configuration = Config.get_configuration_settings();

            container.Register(() => configuration, Lifestyle.Singleton);
            container.Register<IFileSystem, DotNetFileSystem>(Lifestyle.Singleton);
            container.Register<IXmlService, XmlService>(Lifestyle.Singleton);
            //nuget
            container.Register<ILogger, ChocolateyNugetLogger>(Lifestyle.Singleton);
            container.Register<INugetService, NugetService>(Lifestyle.Singleton);
            container.Register<IPowershellService, PowershellService>(Lifestyle.Singleton);
            container.Register<IChocolateyPackageInformationService, ChocolateyPackageInformationService>(Lifestyle.Singleton);
            container.Register<IShimGenerationService, ShimGenerationService>(Lifestyle.Singleton);
            container.Register<IRegistryService, RegistryService>(Lifestyle.Singleton);
            container.Register<IFilesService, FilesService>(Lifestyle.Singleton);
            container.Register<IHashProvider>(() => new CrytpoHashProvider(container.GetInstance<IFileSystem>(), CryptoHashProviderType.Md5), Lifestyle.Singleton);
            container.Register<ITemplateService, TemplateService>(Lifestyle.Singleton);
            container.Register<IChocolateyConfigSettingsService, ChocolateyConfigSettingsService>(Lifestyle.Singleton);
            container.Register<IChocolateyPackageService, ChocolateyPackageService>(Lifestyle.Singleton);
            container.Register<IAutomaticUninstallerService, AutomaticUninstallerService>(Lifestyle.Singleton);
            container.Register<ICommandExecutor, CommandExecutor>(Lifestyle.Singleton);
            container.Register(() => new CustomString(string.Empty));

            //todo:refactor - this should be autowired
            container.Register<IEnumerable<ICommand>>(() =>
                {
                    var list = new List<ICommand>
                        {
                            new ChocolateyListCommand(container.GetInstance<IChocolateyPackageService>()),
                            new ChocolateyInstallCommand(container.GetInstance<IChocolateyPackageService>()),
                            new ChocolateyPinCommand(container.GetInstance<IChocolateyPackageInformationService>(), container.GetInstance<ILogger>(), container.GetInstance<INugetService>()),
                            new ChocolateyUpgradeCommand(container.GetInstance<IChocolateyPackageService>()),
                            new ChocolateyUninstallCommand(container.GetInstance<IChocolateyPackageService>()),
                            new ChocolateyPackCommand(container.GetInstance<IChocolateyPackageService>()),
                            new ChocolateyPushCommand(container.GetInstance<IChocolateyPackageService>(), container.GetInstance<IChocolateyConfigSettingsService>()),
                            new ChocolateyNewCommand(container.GetInstance<ITemplateService>()),
                            new ChocolateySourceCommand(container.GetInstance<IChocolateyConfigSettingsService>()),
                            new ChocolateyFeatureCommand(container.GetInstance<IChocolateyConfigSettingsService>()),
                            new ChocolateyApiKeyCommand(container.GetInstance<IChocolateyConfigSettingsService>()),
                            new ChocolateyUnpackSelfCommand(container.GetInstance<IFileSystem>()),
                            new ChocolateyVersionCommand(container.GetInstance<IChocolateyPackageService>()),
                            new ChocolateyUpdateCommand(container.GetInstance<IChocolateyPackageService>())
                        };
                    return list.AsReadOnly();
                }, Lifestyle.Singleton);

            container.Register<IEventSubscriptionManagerService, EventSubscriptionManagerService>(Lifestyle.Singleton);
            EventManager.initialize_with(container.GetInstance<IEventSubscriptionManagerService>);

            container.Register<IDateTimeService, SystemDateTimeUtcService>(Lifestyle.Singleton);
        }
    }

    // ReSharper restore InconsistentNaming
}