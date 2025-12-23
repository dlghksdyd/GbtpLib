using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("MST_CUSTOMER")]
    public class MstCustomerEntity
    {
        [Key]
        [Column("CSTM_CD")]
        [MaxLength(12)]
        public string CustomerCode { get; set; }

        [Column("CSTM_NM")]
        [MaxLength(60)]
        public string CustomerName { get; set; }
        [Column("CSTM_TYPE")]
        [MaxLength(5)]
        public string CustomerType { get; set; }
        [Column("CSTM_ADR")]
        [MaxLength(100)]
        public string CustomerAddress { get; set; }
        [Column("PHS_SLN_CL")]
        [MaxLength(1)]
        public string PhsSlnCl { get; set; }
        [Column("CSTM_CRGR")]
        [MaxLength(20)]
        public string CustomerManager { get; set; }
        [Column("CSTM_CRGR_EML")]
        [MaxLength(30)]
        public string CustomerManagerEmail { get; set; }
        [Column("USE_YN")]
        [MaxLength(1)]
        public string UseYn { get; set; }
        [Column("NOTE")]
        [MaxLength(255)]
        public string Note { get; set; }
        [Column("REG_DY")]
        public DateTime? RegDay { get; set; }
        [Column("REGR")]
        [MaxLength(10)]
        public string Registrar { get; set; }
        [Column("UPD_DY")]
        public DateTime? UpdateDay { get; set; }
        [Column("UPDR")]
        [MaxLength(10)]
        public string Updater { get; set; }
    }
}
