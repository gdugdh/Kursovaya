//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Kursovaya
{
    using System;
    using System.Collections.Generic;
    
    public partial class PriceCategoryInSession
    {
        public int id { get; set; }
        public int id_session { get; set; }
        public int id_price_category { get; set; }
        public decimal price { get; set; }
    
        public virtual PriceCategory PriceCategory { get; set; }
        public virtual Session Session { get; set; }
    }
}