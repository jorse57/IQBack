using AutoMapper;
using IQ_Api.DTOs;
using IQ_Api.Entidades;

//using MailKit.Net.Smtp;
using MimeKit.Text;
using MimeKit;
//using MailKit.Security;





using IQ_Api.Services;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MailKit.Security;
//using System.Net.Mail;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IQ_Api.Controllers
{
    [Route("api/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IConfiguration _configuration;
        public SmtpClient _smtpClient;


        public static string cadena { get; set; }

        //private readonly IEmailService _emailService;

        public IMapper mapper { get; }

        public UserController(ApplicationDbContext context,
                              UserManager<IdentityUser> userManager,
                              SignInManager<IdentityUser> signInManager,
                              IMapper _mapper,
                              IConfiguration configuration
                             
                              //IEmailService emailService
                              )
        {
            this.context = context;
            this.userManager = userManager;
            this._configuration = configuration;
        
            this.signInManager = signInManager;
            cadena = _configuration.GetConnectionString("defaultConnection");

            mapper = _mapper;
      

            //_emailService = emailService;
        }


        // GET: api/<UserController>
        [HttpGet]
        public async Task<ActionResult<List<CredencialesUsuario>>> Get()
        {
            return await context.Usuarios.ToListAsync();
        }
        [HttpGet]
        public async Task<ActionResult<List<Modulos>>> GetModulos()
        {
            return await context.Modulos.ToListAsync();
        }
        [HttpGet]
        public async Task<ActionResult<List<Submodulos>>> GetSubModulos()
        {
            return await context.Submodulos.ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<List<usuarioRutasDTO>> rutasUsuario(int id )
        {
            List<usuarioRutasDTO> resp = new List<usuarioRutasDTO>();
            try
            {
                SqlConnectionStringBuilder conn = new SqlConnectionStringBuilder();


                using (SqlConnection connection = new SqlConnection(cadena))
                {
                 

                    String sql = $"SELECT usuarios_submodulos.id,usuarios_submodulos.lectura, \r\nusuarios_submodulos.escritura, Usuarios.Id as id_usuario,\r\nSubmodulos.ruta as ruta_submodulo,\r\nSubmodulos.nombre as nombre_submodulo,\r\nModulos.nombreModulo as modulo_padre\r\nFROM usuarios_submodulos\r\nINNER JOIN Usuarios ON usuarios_submodulos.id_usuario=Usuarios.Id\r\nINNER JOIN Submodulos ON usuarios_submodulos.id_submodulo=Submodulos.id\r\nINNER JOIN Modulos ON Submodulos.id_modulo=Modulos.id\r\nWhere Usuarios.Id = '{id}'";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader result = command.ExecuteReader())
                        {
                            while (result.Read())
                            {
                                var j = new usuarioRutasDTO()
                                {
                                    id = result.GetInt32(0),
                                    lectura = result.GetBoolean(1),
                                    escritura = result.GetBoolean(2),
                                    idUsuario = result.GetInt32(3),
                                    rutaSubmodulo = result.IsDBNull(4) ? "" : result.GetString(4),
                                    nombreSubmodulo = result.IsDBNull(5) ? "" : result.GetString(5),
                                    moduloPadre = result.IsDBNull(6) ? "" : result.GetString(6),

                                };

                                resp.Add(j);
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            return resp;
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<CredencialesUsuario>> getUsersById(int id)
        {
            var User = await context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);

            if (User == null)
            {
                return NotFound();
            }
            return mapper.Map<CredencialesUsuario>(User);

        }

        [HttpGet("{email}")]
        public async Task<ActionResult<CredencialesUsuario>> getUsersByEmail(string email)
        {
            var User = await context.Usuarios.FirstOrDefaultAsync(x => x.Email == email);

            if (User == null)
            {
                return NotFound();
            }
            return mapper.Map<CredencialesUsuario>(User);

        }



        [HttpPost]
        public async Task<ActionResult<RespuestaAutenticacion>> createUser([FromBody] CredencialesUsuario credenciales)
        {

            var usuario = new IdentityUser { UserName = credenciales.Email, Email = credenciales.Email };
            var resultado = await userManager.CreateAsync(usuario, credenciales.Password);

            if (resultado.Succeeded)
            {
                context.Add(credenciales);
                await context.SaveChangesAsync();
                return await ConstruirToken(credenciales);
            }
            else
            {
                return BadRequest(resultado.Errors);
            }

        }

        [HttpPost]
        public async Task<ActionResult> createSubmodulo([FromBody] Submodulos submodulo)
        {
            if (submodulo != null)
            {
                context.Add(submodulo);
                await context.SaveChangesAsync();
                return Ok("submodulo creado");
            }
            else
            {
                return BadRequest("error al crear submodulo");
            }

        }


        [HttpPost()]
        public async Task<ActionResult<RespuestaAutenticacion>> Login([FromBody] CredencialesUsuario credenciales)
        {
            var resultado = await signInManager.PasswordSignInAsync(credenciales.Email, credenciales.Password,
                isPersistent: false, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credenciales);
            }
            else
            {
                return BadRequest("Email/Contraseña incorrectos");
            }
        }

        private async Task<RespuestaAutenticacion> ConstruirToken(CredencialesUsuario credenciales)
        {
            var claims = new List<Claim>()
            {
                new Claim ("email", credenciales.Email)
            };

            var usuario = await userManager.FindByEmailAsync(credenciales.Email);
            var claimsDB = await userManager.GetClaimsAsync(usuario);

            claims.AddRange(claimsDB);

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["llaveJWT"]));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1);

            var token = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiracion, signingCredentials: creds);


            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiracion = expiracion,

            };


        }

        [HttpPut("{id}")]
        public async Task<ActionResult> updateUser(int id, CreacionUsuarioDTO creacionUsuarioDTO)
        {
            var userExist = await context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
            if (userExist == null)
            {
                return NotFound();
            }


            userExist = mapper.Map(creacionUsuarioDTO, userExist);
            var rolUpdate = context.Update(userExist);
            await context.SaveChangesAsync();
            return Ok();
        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteUser(int id)
        {

            var userExist = await context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);

            if (userExist == null)
            {
                return NotFound();
            }
            var removeUser = context.Remove(userExist);
            await context.SaveChangesAsync();
            return Ok(id);
        }


        [HttpPut]
        public ActionResult<string> sendMail(string to, string subject)
        {
            try
            {
                var resp = "Email enviado correctamente, verificar correo electronico";
                // create message
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse("iqcostos@outlook.es"));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                string template = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\" style=\"font-family:arial, 'helvetica neue', helvetica, sans-serif\">\r\n<head>\r\n<meta charset=\"UTF-8\">\r\n<meta content=\"width=device-width, initial-scale=1\" name=\"viewport\">\r\n<meta name=\"x-apple-disable-message-reformatting\">\r\n<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\r\n<meta content=\"telephone=no\" name=\"format-detection\">\r\n<title>Nueva plantilla de correo electrC3B3nico 2023-03-22</title><!--[if (mso 16)]>\r\n<style type=\"text/css\">\r\na {text-decoration: none;}\r\n</style>\r\n<![endif]--><!--[if gte mso 9]><style>sup { font-size: 100% !important; }</style><![endif]--><!--[if gte mso 9]>\r\n<xml>\r\n<o:OfficeDocumentSettings>\r\n<o:AllowPNG></o:AllowPNG>\r\n<o:PixelsPerInch>96</o:PixelsPerInch>\r\n</o:OfficeDocumentSettings>\r\n</xml>\r\n<![endif]-->\r\n<style type=\"text/css\">\r\n#outlook a {\r\npadding:0;\r\n}\r\n.es-button {\r\nmso-style-priority:100!important;\r\ntext-decoration:none!important;\r\n}\r\na[x-apple-data-detectors] {\r\ncolor:inherit!important;\r\ntext-decoration:none!important;\r\nfont-size:inherit!important;\r\nfont-family:inherit!important;\r\nfont-weight:inherit!important;\r\nline-height:inherit!important;\r\n}\r\n.es-desk-hidden {\r\ndisplay:none;\r\nfloat:left;\r\noverflow:hidden;\r\nwidth:0;\r\nmax-height:0;\r\nline-height:0;\r\nmso-hide:all;\r\n}\r\n@media only screen and (max-width:600px) {p, ul li, ol li, a { line-height:150%!important } h1, h2, h3, h1 a, h2 a, h3 a { line-height:120% } h1 { font-size:30px!important; text-align:left } h2 { font-size:24px!important; text-align:left } h3 { font-size:20px!important; text-align:left } .es-header-body h1 a, .es-content-body h1 a, .es-footer-body h1 a { font-size:30px!important; text-align:left } .es-header-body h2 a, .es-content-body h2 a, .es-footer-body h2 a { font-size:24px!important; text-align:left } .es-header-body h3 a, .es-content-body h3 a, .es-footer-body h3 a { font-size:20px!important; text-align:left } .es-menu td a { font-size:14px!important } .es-header-body p, .es-header-body ul li, .es-header-body ol li, .es-header-body a { font-size:14px!important } .es-content-body p, .es-content-body ul li, .es-content-body ol li, .es-content-body a { font-size:14px!important } .es-footer-body p, .es-footer-body ul li, .es-footer-body ol li, .es-footer-body a { font-size:14px!important } .es-infoblock p, .es-infoblock ul li, .es-infoblock ol li, .es-infoblock a { font-size:12px!important } *[class=\"gmail-fix\"] { display:none!important } .es-m-txt-c, .es-m-txt-c h1, .es-m-txt-c h2, .es-m-txt-c h3 { text-align:center!important } .es-m-txt-r, .es-m-txt-r h1, .es-m-txt-r h2, .es-m-txt-r h3 { text-align:right!important } .es-m-txt-l, .es-m-txt-l h1, .es-m-txt-l h2, .es-m-txt-l h3 { text-align:left!important } .es-m-txt-r img, .es-m-txt-c img, .es-m-txt-l img { display:inline!important } .es-button-border { display:inline-block!important } a.es-button, button.es-button { font-size:18px!important; display:inline-block!important } .es-adaptive table, .es-left, .es-right { width:100%!important } .es-content table, .es-header table, .es-footer table, .es-content, .es-footer, .es-header { width:100%!important; max-width:600px!important } .es-adapt-td { display:block!important; width:100%!important } .adapt-img { width:100%!important; height:auto!important } .es-m-p0 { padding:0px!important } .es-m-p0r { padding-right:0px!important } .es-m-p0l { padding-left:0px!important } .es-m-p0t { padding-top:0px!important } .es-m-p0b { padding-bottom:0!important } .es-m-p20b { padding-bottom:20px!important } .es-mobile-hidden, .es-hidden { display:none!important } tr.es-desk-hidden, td.es-desk-hidden, table.es-desk-hidden { width:auto!important; overflow:visible!important; float:none!important; max-height:inherit!important; line-height:inherit!important } tr.es-desk-hidden { display:table-row!important } table.es-desk-hidden { display:table!important } td.es-desk-menu-hidden { display:table-cell!important } .es-menu td { width:1%!important } table.es-table-not-adapt, .esd-block-html table { width:auto!important } table.es-social { display:inline-block!important } table.es-social td { display:inline-block!important } .es-desk-hidden { display:table-row!important; width:auto!important; overflow:visible!important; max-height:inherit!important } }\r\n</style>\r\n</head>\r\n<body style=\"width:100%;font-family:arial, 'helvetica neue', helvetica, sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0\">\r\n<div class=\"es-wrapper-color\" style=\"background-color:#F6F6F6\"><!--[if gte mso 9]>\r\n<v:background xmlns:v=\"urn:schemas-microsoft-com:vml\" fill=\"t\">\r\n<v:fill type=\"tile\" color=\"#f6f6f6\"></v:fill>\r\n</v:background>\r\n<![endif]-->\r\n<table class=\"es-wrapper\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-repeat:repeat;background-position:center top;background-color:#F6F6F6\">\r\n<tr>\r\n<td valign=\"top\" style=\"padding:0;Margin:0\">\r\n<table class=\"es-header\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top\">\r\n<tr>\r\n<td align=\"center\" style=\"padding:0;Margin:0\">\r\n<table class=\"es-header-body\" cellspacing=\"0\" cellpadding=\"0\" bgcolor=\"#ffffff\" align=\"center\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#FFFFFF;width:600px\">\r\n<tr>\r\n<td align=\"left\" style=\"padding:0;Margin:0;padding-top:20px;padding-left:20px;padding-right:20px\"><!--[if mso]><table style=\"width:560px\" cellpadding=\"0\"\r\ncellspacing=\"0\"><tr><td style=\"width:180px\" valign=\"top\"><![endif]-->\r\n<table class=\"es-left\" cellspacing=\"0\" cellpadding=\"0\" align=\"left\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;float:left\">\r\n<tr>\r\n<td class=\"es-m-p0r es-m-p20b\" valign=\"top\" align=\"center\" style=\"padding:0;Margin:0;width:180px\">\r\n<table width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" role=\"presentation\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px\">\r\n<tr>\r\n<td align=\"left\" style=\"padding:0;Margin:0\"><p style=\"Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;color:#333333;font-size:14px\">Bienvenid@ a IQcostos</p><p style=\"Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;color:#333333;font-size:14px\"><br></p></td>\r\n</tr>\r\n</table></td>\r\n</tr>\r\n</table><!--[if mso]></td><td style=\"width:20px\"></td><td style=\"width:360px\" valign=\"top\"><![endif]-->\r\n<table cellspacing=\"0\" cellpadding=\"0\" align=\"right\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px\">\r\n<tr>\r\n<td align=\"left\" style=\"padding:0;Margin:0;width:360px\">\r\n<table width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px\">\r\n<tr>\r\n<td style=\"padding:0;Margin:0;display:none\" align=\"center\"></td>\r\n</tr>\r\n</table></td>\r\n</tr>\r\n</table><!--[if mso]></td></tr></table><![endif]--></td>\r\n</tr>\r\n</table></td>\r\n</tr>\r\n</table>\r\n<table class=\"es-content\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%\">\r\n<tr>\r\n<td align=\"center\" style=\"padding:0;Margin:0\">\r\n<table class=\"es-content-body\" cellspacing=\"0\" cellpadding=\"0\" bgcolor=\"#ffffff\" align=\"center\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#FFFFFF;width:600px\">\r\n<tr>\r\n<td align=\"left\" style=\"padding:0;Margin:0;padding-top:20px;padding-left:20px;padding-right:20px\">\r\n<table width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px\">\r\n<tr>\r\n<td valign=\"top\" align=\"center\" style=\"padding:0;Margin:0;width:560px\">\r\n<table width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" role=\"presentation\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px\">\r\n<tr>\r\n<td style=\"padding:0;Margin:0\"><h1 style=\"Margin:0;line-height:36px;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;font-size:30px;font-style:normal;font-weight:normal;color:#333333\">Enlace de autenticación</h1><p style=\"Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:arial, 'helvetica neue', helvetica, sans-serif;line-height:21px;color:#333333;font-size:14px\">Hola, el siguiente enlace te permitira terminar tu registro: <br> https://www.youtube.com/watch?v=BxuliG7UNWs&ab_channel=Swhite_exe</p></td>\r\n</tr>\r\n</table></td>\r\n</tr>\r\n</table></td>\r\n</tr>\r\n</table></td>\r\n</tr>\r\n</table>\r\n<table class=\"es-footer\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:transparent;background-repeat:repeat;background-position:center top\">\r\n<tr>\r\n<td align=\"center\" style=\"padding:0;Margin:0\">\r\n<table class=\"es-footer-body\" cellspacing=\"0\" cellpadding=\"0\" bgcolor=\"#ffffff\" align=\"center\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#FFFFFF;width:600px\">\r\n<tr>\r\n<td align=\"left\" style=\"Margin:0;padding-top:20px;padding-bottom:20px;padding-left:20px;padding-right:20px\"><!--[if mso]><table style=\"width:560px\" cellpadding=\"0\"\r\ncellspacing=\"0\"><tr><td style=\"width:270px\" valign=\"top\"><![endif]-->\r\n<table class=\"es-left\" cellspacing=\"0\" cellpadding=\"0\" align=\"left\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;float:left\">\r\n<tr>\r\n<td class=\"es-m-p20b\" align=\"left\" style=\"padding:0;Margin:0;width:270px\">\r\n<table width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" role=\"presentation\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px\">\r\n<tr>\r\n<td align=\"center\" style=\"padding:0;Margin:0;font-size:0px\"><img class=\"adapt-img\" src=\"https://dlcfvm.stripocdn.email/content/guids/c8a99fe9-0579-4a46-9380-9beeaf3c264d/images/iqlogo.png\" alt style=\"display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic\" width=\"270\"></td>\r\n</tr>\r\n</table></td>\r\n</tr>\r\n</table><!--[if mso]></td><td style=\"width:20px\"></td><td style=\"width:270px\" valign=\"top\"><![endif]-->\r\n<table class=\"es-right\" cellspacing=\"0\" cellpadding=\"0\" align=\"right\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;float:right\">\r\n<tr>\r\n<td align=\"left\" style=\"padding:0;Margin:0;width:270px\">\r\n<table width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px\">\r\n<tr>\r\n<td align=\"center\" style=\"padding:0;Margin:0;display:none\"></td>\r\n</tr>\r\n</table></td>\r\n</tr>\r\n</table><!--[if mso]></td></tr></table><![endif]--></td>\r\n</tr>\r\n</table></td>\r\n</tr>\r\n</table></td>\r\n</tr>\r\n</table>\r\n</div>\r\n</body>\r\n</html>";
                email.Body = new TextPart(TextFormat.Html) { Text = $"{template}" };

                // send email
                using var smtp = new SmtpClient();
                smtp.Connect("smtp-mail.outlook.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate("iqcostos@outlook.es", "Stuntlomejor159");
                smtp.Send(email);
                smtp.Disconnect(true);

                return resp;

            }
            catch (Exception ex)
            {
                throw;
            }
        }



    }
}
