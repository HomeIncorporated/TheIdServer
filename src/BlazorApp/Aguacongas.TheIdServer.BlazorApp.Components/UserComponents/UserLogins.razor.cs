﻿using Aguacongas.IdentityServer.Store.Entity;
using System.Collections.Generic;
using System.Linq;

namespace Aguacongas.TheIdServer.BlazorApp.Components.UserComponents
{
    public partial class UserLogins
    {
        private IEnumerable<UserLogin> Logins => Collection.Where(l => l.ProviderDisplayName.Contains(HandleModificationState.FilterTerm));
    }
}
