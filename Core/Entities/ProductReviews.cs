namespace Core.Entities
{
    public class ProductReview : BaseEntity
    {
       public int ProductId { get; set; }
        public string UserName { get; set; }
         public string Summary { get; set; }
          public string Review { get; set; }
    }
}