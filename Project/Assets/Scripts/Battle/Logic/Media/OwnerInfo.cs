using System;
using Battle.Logic.AllManager;
using cfg;
using Configs;

namespace Battle.Logic.Media
{
    

    public readonly struct OwnerInfo
    {
        public readonly BodyL Owner;

        public OwnerInfo(BodyL owner)
        {
            Owner = owner;
        }
    }
}