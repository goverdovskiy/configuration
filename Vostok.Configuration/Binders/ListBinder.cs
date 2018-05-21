﻿using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration.Binders
{
    internal class ListBinder<T> :
        ISettingsBinder<List<T>>,
        ISettingsBinder<IList<T>>,
        ISettingsBinder<IEnumerable<T>>,
        ISettingsBinder<ICollection<T>>,
        ISettingsBinder<IReadOnlyCollection<T>>,
        ISettingsBinder<IReadOnlyList<T>>
    {
        private readonly ISettingsBinder<T> elementBinder;

        public ListBinder(ISettingsBinder<T> elementBinder)
        {
            this.elementBinder = elementBinder;
        }

        public List<T> Bind(RawSettings settings)
        {
            RawSettings.CheckSettings(settings);

            return (settings.Children ?? Enumerable.Empty<RawSettings>())
                .Select(n => elementBinder.Bind(n))
                .ToList();
        }

        IList<T> ISettingsBinder<IList<T>>.Bind(RawSettings settings) => Bind(settings);
        IEnumerable<T> ISettingsBinder<IEnumerable<T>>.Bind(RawSettings settings) => Bind(settings);
        ICollection<T> ISettingsBinder<ICollection<T>>.Bind(RawSettings settings) => Bind(settings);
        IReadOnlyCollection<T> ISettingsBinder<IReadOnlyCollection<T>>.Bind(RawSettings settings) => Bind(settings);
        IReadOnlyList<T> ISettingsBinder<IReadOnlyList<T>>.Bind(RawSettings settings) => Bind(settings);
    }
}