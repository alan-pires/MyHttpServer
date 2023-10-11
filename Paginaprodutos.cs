using System.Text;

class Paginaprodutos : PaginaDinamica
{
    public override byte[] Get(SortedList<string, string> parametros)
    {
        StringBuilder htmlGerado = new StringBuilder();

        string codigo = parametros.ContainsKey("id") ? parametros["id"] : "";

        if (!string.IsNullOrEmpty(codigo))
        {
            foreach(var p in Produto.Listagem)
            {
                if (p.Codigo == Convert.ToInt32(parametros["id"]))
                {
                    htmlGerado.Append("<tr>");
                    htmlGerado.Append($"<td><b>{p.Codigo}</b></td>");
                    htmlGerado.Append($"<td><b>{p.Nome}</b></td>");
                    htmlGerado.Append("</tr>");
                }
            }
        }
        else
        {
            foreach(var p in Produto.Listagem)
            {
                htmlGerado.Append("<tr>");
                htmlGerado.Append($"<td><b>{p.Codigo}</b></td>");
                htmlGerado.Append($"<td><b>{p.Nome}</b></td>");
                htmlGerado.Append("</tr>");
            }
        }

        string textoHtmlGerado = this.HtmlModelo.Replace("{{HtmlGerado}}", htmlGerado.ToString());
        return Encoding.UTF8.GetBytes(textoHtmlGerado);
    }
}