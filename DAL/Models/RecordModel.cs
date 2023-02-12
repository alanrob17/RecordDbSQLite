using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class RecordModel
    {
        [Key]
        public int RecordId { get; set; }

        [Required]  
        public int ArtistId { get; set; }

        [StringLength(80)]
        public string? Name { get; set; }
        
        [StringLength(50)]
        public string? Field { get; set; }
        
        public int Recorded { get; set; }
        
        [StringLength(50)]
        public string? Label { get; set; }
        
        [StringLength(50)]
        public string? Pressing { get; set; }
        
        [StringLength(4)]
        public string? Rating { get; set; }
        public int Discs { get; set; }
        
        [StringLength(50)]
        public string? Media { get; set; }
        
        [Column(TypeName = "text")]
        public string? Bought { get; set; }
        
        [Column(TypeName = "money")]
        public decimal? Cost { get; set; }
        
        [Column(TypeName = "text")]
        public string? Review { get; set; }
        
        [ForeignKey(nameof(ArtistId))]
        [InverseProperty("Records")]
        public virtual ArtistModel? Artist { get; set; }

        public override string ToString()
        {
            return $"Record Id: {RecordId}, Name: {Name}, Recorded: {Recorded}, Media: {Media}";
        }
    }
}
