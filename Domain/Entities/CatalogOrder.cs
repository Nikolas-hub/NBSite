using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CatalogOrder
{
    public long Id { get; set; }

    public long Status { get; set; }

    public string? Reciever { get; set; } = null!;

    public string? Email { get; set; } = null!;

    public string? Comment { get; set; }

    public long? DeliveryId { get; set; }

    public long? PaymentId { get; set; }

    public long? UserId { get; set; }

    public long? CityId { get; set; }

    public long? CouponId { get; set; }

    public string? Phone { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public virtual ICollection<CatalogOrderproduct> CatalogOrderproducts { get; set; } = new List<CatalogOrderproduct>();

    public virtual ReferencesCity? City { get; set; }

    public virtual CatalogDelivery? Delivery { get; set; }

    public virtual CatalogPayment? Payment { get; set; }

    public virtual AuthUser? User { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual CatalogCoupon? Coupon { get; set; }
}
