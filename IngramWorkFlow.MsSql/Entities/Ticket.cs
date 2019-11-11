namespace IngramWorkFlow.MsSql
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
 

    [Table("Ticket")]
    public partial class Ticket
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Ticket()
        {
            DocumentTransitionHistories = new HashSet<TicketTransitionHistory>();
            State = "RequestCreated";
            StateName = "Request created";
        }

        public Guid Id { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Number { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        public string Comment { get; set; }

        public Guid AuthorId { get; set; }

        public Guid? ManagerId { get; set; }

        public decimal Sum { get; set; }

        [Required]
        [StringLength(1024)]
        public string State { get; set; }

        [StringLength(1024)]
        public string StateName { get; set; }

        public virtual User Author { get; set; }

        public virtual User Manager { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TicketTransitionHistory> DocumentTransitionHistories { get; set; }
    }
}
