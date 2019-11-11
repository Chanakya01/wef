namespace IngramWorkFlow.MsSql
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("TicketTransitionHistory")]
    public partial class TicketTransitionHistory
    {
        public Guid Id { get; set; }

        public Guid TicketId { get; set; }

        public Guid? UserId { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string AllowedToUserNames { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? TransitionTime { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Order { get; set; }

        [Required]
        [StringLength(1024)]
        public string InitialState { get; set; }

        [Required]
        [StringLength(1024)]
        public string DestinationState { get; set; }

        [Required]
        [StringLength(1024)]
        public string Command { get; set; }

        public virtual Ticket Document { get; set; }

        public virtual User User { get; set; }
    }
}
