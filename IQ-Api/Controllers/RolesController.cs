//using AutoMapper;
//using IQ_Api.DTOs;
//using IQ_Api.Entidades;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace IQ_Api.Controllers
//{
//    [Route("api/[action]")]
//    [ApiController]
//    public class RolesController : ControllerBase
//    {
//        public ApplicationDbContext context { get; }
//        private  IMapper mapper { get; }

//        public RolesController(ApplicationDbContext _context, IMapper _mapper)
//        {
//            context = _context;
//            mapper = _mapper;
//        }


//        [HttpGet]
//        public async Task<ActionResult<List<Roles>>> getRoles()
//        {
//            return await context.Roles.ToListAsync();
//        }


//        [HttpGet("{id:int}")]
//        public async Task<ActionResult<rolesDTO>> getRolesById(int id)
//        {
//            var Rol = await context.Roles.FirstOrDefaultAsync(x => x.Id == id);

//            if (Rol == null)
//            {
//                return NotFound();
//            }
//            return mapper.Map<rolesDTO>(Rol);

//        }

//        [HttpPost]
//        public async Task<ActionResult> createRole([FromBody] Roles rol)
//        {
//            if (!rol.Equals("") && rol != null)
//            {
//                context.Roles.Add(rol);

//                var respuesta = await context.SaveChangesAsync();
//                return Ok(respuesta);

//            }
//            return BadRequest("error creando rol");
//        }

//        [HttpPut("{id}")]
//        public async Task<ActionResult> updateRole(int id,  rolesCreacionDTO rolCreacionDTO)
//        {
//            var Rol = await context.Roles.FirstOrDefaultAsync(x => x.Id == id);
//            if (Rol == null)
//            {
//                return NotFound();
//            }


//            Rol = mapper.Map(rolCreacionDTO, Rol);
//            var rolUpdate =  context.Update(Rol);
//            await context.SaveChangesAsync();
//            return Ok();
//        }

//        [HttpDelete("{id:int}")]
//        public async Task<ActionResult> DeleteRol(int id)
//        {
         
//             var rolExist = await context.Roles.FirstOrDefaultAsync(x => x.Id == id);

//            if (rolExist == null )
//            {
//                return NotFound();
//            }
//            var removeRol = context.Remove(rolExist);
//            await context.SaveChangesAsync();
//            return Ok(id);
//        }
//    }
//}
