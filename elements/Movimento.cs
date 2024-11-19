using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace luftetl.elements
{
    public class Movimento
{
    public int Id_Movimento { get; set; }
    public string Nr_Conhecimento { get; set; }
    public string Ds_TipoMovimento { get; set; }
    public string Luft { get; set; }
    public string Negocio { get; set; }
    public int Id_Pessoa { get; set; }
    public string Cd_CGCCPF { get; set; }
    public string Ds_Cliente { get; set; }
    public string CidFilial { get; set; }
    public DateTime Emissao { get; set; }
    public bool Faturado { get; set; }
    public string CnpjFaturado { get; set; }
    public string RaizFaturado { get; set; }
    public string Ds_Cidade { get; set; }
    public string Ds_Estado { get; set; }
    public string Ds_Entrega { get; set; }
    public decimal PesoCubado { get; set; }
    public decimal PesoReal { get; set; }
    public decimal VlrMercadoria { get; set; }
    public decimal Vl_Frete { get; set; }
    public decimal Vl_IssIcms { get; set; }
    public decimal Vl_PisCofins { get; set; }
    public decimal Vl_Liquido { get; set; }
    public decimal Aliquota { get; set; }
    public int Id_ManifestoEntrega { get; set; }
    public int Id_ManifestoViagem { get; set; }
    public string FilialAtual { get; set; }
    public string Status_Mov { get; set; }
    public string Regional { get; set; }
}

}
