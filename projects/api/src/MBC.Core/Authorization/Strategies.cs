using System.Collections.Generic;
using MBC.Core.Entities;

namespace MBC.Core.Authorization;

public static class Strategies
{
    public static StakeholderAuthorizationStrategy DeliveryReview { get; }
    = new StakeholderAuthorizationStrategy
    {
        ResourceType = typeof(DeliveryReview),
        Anonymous = AuthorizedFor.View,
        Pilot = AuthorizedFor.View,
        Customer = AuthorizedFor.View | AuthorizedFor.Mutate,
        Admin = AuthorizedFor.View | AuthorizedFor.Mutate
    };

    public static StakeholderAuthorizationStrategy Delivery { get; }
    = new StakeholderAuthorizationStrategy
    {
        ResourceType = typeof(Delivery),
        Anonymous = AuthorizedFor.Nothing,
        Pilot = AuthorizedFor.View,
        Customer = AuthorizedFor.View | AuthorizedFor.Mutate,
        Admin = AuthorizedFor.View | AuthorizedFor.Mutate
    };

    public static StakeholderAuthorizationStrategy DeliveryProof { get; }
    = new StakeholderAuthorizationStrategy
    {
        ResourceType = typeof(DeliveryProof),
        Anonymous = AuthorizedFor.Nothing,
        Pilot = AuthorizedFor.View | AuthorizedFor.Mutate,
        Customer = AuthorizedFor.View,
        Admin = AuthorizedFor.View | AuthorizedFor.Mutate
    };

    public static IEnumerable<StakeholderAuthorizationStrategy> All { get; }
        = [DeliveryReview, Delivery, DeliveryProof];
}

