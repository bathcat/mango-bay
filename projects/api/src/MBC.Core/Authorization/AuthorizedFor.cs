using System;

namespace MBC.Core.Authorization;

[Flags]
public enum AuthorizedFor
{
    Nothing = 0,
    View = 1,
    Mutate = 2
}

