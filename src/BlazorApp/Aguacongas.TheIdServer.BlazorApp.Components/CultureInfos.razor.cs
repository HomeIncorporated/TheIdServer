﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Aguacongas.TheIdServer.BlazorApp.Components
{
    public partial class CultureInfos
    {
        private IEnumerable<CultureInfo> _filterValues;
        protected override bool IsReadOnly => true;

        protected override string PropertyName => "Name";

        protected override Task<IEnumerable<string>> GetFilteredValues(string term)
        {
            term = term ?? string.Empty;
            _filterValues = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Where(c => c.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || c.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Take(5);

            return Task.FromResult(_filterValues.Select(c => c.Name));
        }

        protected override void SetValue(string inputValue)
        {
            var cultureInfo = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .FirstOrDefault(c => c.Name == inputValue);
            if (cultureInfo != null)                
            {
                Entity = cultureInfo;
            }
        }
    }
}
