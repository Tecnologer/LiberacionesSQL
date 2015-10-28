using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibercionesCSharp
{
    public class Entidad
    {
        public int LiberacionID { get; set; }
        public int Folio { get; set; }
        public int LiberacionEstatus { get; set; }
        public string LiberacionEstatusString { get; set; } 
        public int LiberacionDetalleID { get; set; }
        public string Nombre { get; set; }
        public string Script { get; set; }
        public int DetalleEstatus { get; set; }
        public string DetalleEstatusString { get; set; }
        public string Error { get; set; }
        public bool Nuevo { get; set; }
        public bool Modificado { get; set; }
        public string Descripcion { get; set; }
    }


    public enum estatus
    {
        Activo = 1,        
        Liberado = 2,
        Error = 3
    }
}
