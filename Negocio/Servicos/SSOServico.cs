namespace CFC_Negocio.Servicos
{
    public class SSOService
    {
        public void Login()
        {
            Autenticacao.Oauth oauth = new Autenticacao.Oauth();
            oauth.Login();
        }

        public Autenticacao.DTO.TokenDTO Autenticar(string code)
        {
            Autenticacao.Oauth oauth = new Autenticacao.Oauth();
            Autenticacao.DTO.TokenDTO token = oauth.Autenticar(code);

            return token;
        }

        public void Sair(string token)
        {
            new Autenticacao.Oauth().Logout(token);
        }

        public Autenticacao.DTO.ClientSecretIdDTO ObterClientSecretId(Autenticacao.DTO.ClienteDTO clienteDTO, string token)
        {
            Autenticacao.Oauth oauth = new Autenticacao.Oauth();
            Autenticacao.DTO.ClientSecretIdDTO clientSecret = oauth.ObterClientSecretId(clienteDTO, token);
            return clientSecret;
        }

        public Autenticacao.DTO.UsuarioDTO Permissao(string token)
        {
            Autenticacao.Oauth oauth = new Autenticacao.Oauth();
            if (token != null)
            {
                Autenticacao.DTO.UsuarioDTO usuarioDTO = oauth.ObterPermissao(token);
                return usuarioDTO;
            }
            else
            {
                Login();
                return null;
            }
        }

        public Autenticacao.DTO.TokenJWTDTO DecodeToken(string token)
        {
            Autenticacao.Oauth oauth = new Autenticacao.Oauth();
            if (token != null)
            {
                Autenticacao.DTO.TokenJWTDTO tokenDecode = oauth.JwtDecode(token);
                return tokenDecode;
            }
            else
            {
                return null;
            }
        }
    }
}