using System.Text;

class Paginaprodutos : PaginaDinamica
{
    public override byte[] Get(SortedList<string, string> parametros)
    {
        StringBuilder htmlGerado = new StringBuilder();

        string codigo = parametros.ContainsKey("id") ? parametros["id"] : "";

        foreach(var p in Produto.Listagem)
        {
            bool negrito = (!string.IsNullOrEmpty(codigo) && codigo == p.Codigo.ToString());

            htmlGerado.Append("<tr>");
            if (negrito)
            {
                htmlGerado.Append($"<td><b>{p.Codigo}</b></td>");
                htmlGerado.Append($"<td><b>{p.Nome}</b></td>");
            }
            else
            {
                htmlGerado.Append($"<td>{p.Codigo}</td>");
                htmlGerado.Append($"<td>{p.Nome}</td>");
            }
            htmlGerado.Append("</tr>");
        }

        string textoHtmlGerado = this.HtmlModelo.Replace("{{HtmlGerado}}", htmlGerado.ToString());
        return Encoding.UTF8.GetBytes(textoHtmlGerado);
    }
}