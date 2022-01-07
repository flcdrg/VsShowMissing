using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

#pragma warning disable S1128 // Unused "using" should be removed
using Microsoft; // Required by VS2019
#pragma warning restore S1128 // Unused "using" should be removed
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
#pragma warning disable S1128 // Unused "using" should be removed
using Microsoft.VisualStudio.Shell.Interop; // Required by VS2019
#pragma warning restore S1128 // Unused "using" should be removed
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Gardiner.VsShowMissing.Options
{
    /// <summary>
    /// A base class for specifying options
    /// </summary>
    internal abstract class BaseOptionModel<T> where T : BaseOptionModel<T>, new()
    {
        private static readonly AsyncLazy<T> _liveModel = new AsyncLazy<T>(CreateAsync, ThreadHelper.JoinableTaskFactory);
#pragma warning disable S2743 // Static fields should not be used in generic types
        private static readonly AsyncLazy<ShellSettingsManager> _settingsManager = new AsyncLazy<ShellSettingsManager>(GetSettingsManagerAsync, ThreadHelper.JoinableTaskFactory);
#pragma warning restore S2743 // Static fields should not be used in generic types

        /// <summary>
        /// A singleton instance of the options. MUST be called from UI thread only.
        /// </summary>
        /// <remarks>
        /// Call <see cref="GetLiveInstanceAsync()" /> instead if on a background thread or in an async context on the main thread.
        /// </remarks>
        public static T Instance
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

#pragma warning disable VSTHRD104 // Offer async methods
                return ThreadHelper.JoinableTaskFactory.Run(GetLiveInstanceAsync);
#pragma warning restore VSTHRD104 // Offer async methods
            }
        }

        /// <summary>
        /// Get the singleton instance of the options. Thread safe.
        /// </summary>
        public static Task<T> GetLiveInstanceAsync() => _liveModel.GetValueAsync();

        /// <summary>
        /// Creates a new instance of the options class and loads the values from the store. For internal use only
        /// </summary>
        /// <returns></returns>
        public static async Task<T> CreateAsync()
        {
            var instance = new T();
            await instance.LoadAsync();
            return instance;
        }

        /// <summary>
        /// The name of the options collection as stored in the registry.
        /// </summary>
        protected virtual string CollectionName { get; } = typeof(T).FullName;

        /// <summary>
        /// Hydrates the properties from the registry.
        /// </summary>
        public virtual void Load()
        {
            ThreadHelper.JoinableTaskFactory.Run(LoadAsync);
        }

        /// <summary>
        /// Hydrates the properties from the registry asyncronously.
        /// </summary>
        public virtual async Task LoadAsync()
        {
            ShellSettingsManager manager = await _settingsManager.GetValueAsync();
            SettingsStore settingsStore = manager.GetReadOnlySettingsStore(SettingsScope.UserSettings);

            if (!settingsStore.CollectionExists(CollectionName))
            {
                return;
            }

            foreach (PropertyInfo property in GetOptionProperties())
            {
                try
                {
                    string serializedProp = settingsStore.GetString(CollectionName, property.Name);
                    object value = DeserializeValue(serializedProp, property.PropertyType);
                    property.SetValue(this, value);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    System.Diagnostics.Debug.Write(ex);
                }
            }
        }

        /// <summary>
        /// Saves the properties to the registry.
        /// </summary>
        public virtual void Save()
        {
            ThreadHelper.JoinableTaskFactory.Run(SaveAsync);
        }

        /// <summary>
        /// Saves the properties to the registry asyncronously.
        /// </summary>
        public virtual async Task SaveAsync()
        {
            ShellSettingsManager manager = await _settingsManager.GetValueAsync();
            WritableSettingsStore settingsStore = manager.GetWritableSettingsStore(SettingsScope.UserSettings);

            if (!settingsStore.CollectionExists(CollectionName))
            {
                settingsStore.CreateCollection(CollectionName);
            }

            foreach (PropertyInfo property in GetOptionProperties())
            {
                string output = SerializeValue(property.GetValue(this));
                settingsStore.SetString(CollectionName, property.Name, output);
            }

            T liveModel = await GetLiveInstanceAsync();

            if (this != liveModel)
            {
                await liveModel.LoadAsync();
            }
        }

        /// <summary>
        /// Serializes an object value to a string
        /// </summary>
        protected virtual string SerializeValue(object value)
        {
            switch (value)
            {
                case null:
                    return string.Empty;
                case string s:
                    return s;
                case bool b:
                    return b.ToString(CultureInfo.InvariantCulture);
            }

            if (value.GetType().IsEnum)
            {
                return value.ToString();
            }

            // last resort
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, value);
                stream.Flush();
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        /// <summary>
        /// Deserializes a string to an object
        /// </summary>
        protected virtual object DeserializeValue(string value, Type type)
        {
            if (type == typeof(string))
            {
                return value;
            }

            if (type == typeof(bool))
            {
                return bool.Parse(value);
            }

            if (type.IsEnum)
            {
                return Enum.Parse(type, value);
            }

            // last resort
            byte[] b = Convert.FromBase64String(value);

            using (var stream = new MemoryStream(b))
            {
                var formatter = new BinaryFormatter();
#pragma warning disable S5773 // Types allowed to be deserialized should be restricted
                return formatter.Deserialize(stream);
#pragma warning restore S5773 // Types allowed to be deserialized should be restricted
            }
        }

#pragma warning disable 1998
        private static async Task<ShellSettingsManager> GetSettingsManagerAsync()
#pragma warning restore 1998
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

#if VS2019 || VS2022
            var svc = await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(SVsSettingsManager)) as IVsSettingsManager;

            Assumes.Present(svc);
            return new ShellSettingsManager(svc);
#else
            return new ShellSettingsManager(ServiceProvider.GlobalProvider);
#endif
        }

        private IEnumerable<PropertyInfo> GetOptionProperties()
        {
            return GetType()
                .GetProperties()
                .Where(p => p.PropertyType.IsSerializable && p.PropertyType.IsPublic);
        }
    }
}