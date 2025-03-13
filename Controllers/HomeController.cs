using Microsoft.AspNetCore.Mvc;
using Supabase;
using Supabase.Gotrue;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using Supabase.Postgrest;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class HomeController : ControllerBase
{
    private readonly Supabase.Client _supabase;

    public HomeController(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    [HttpPost("CreateUser")]
    public async Task<IActionResult> CreateUser([FromBody] UserDto userDto)
    {
        try
        {
            var user = new Users
            {
                IsAluno = userDto.IsAluno,
                Nome = userDto.Nome,
                Senha = userDto.Senha,
                Email = userDto.Email
            };
                user.Id = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
                var response = await _supabase.From<Users>().Insert(user);

            return Ok(new { message = "Usuário inserido com sucesso!", data = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Erro ao inserir usuário", details = ex.Message });
        }
    }

    [HttpPost("GetLogin")]
    public async Task<IActionResult> GetLogin([FromBody] LoginDto loginDto)
    {
        try
        {
            var user = await _supabase
                .From<Users>()
                .Filter("Email", Supabase.Postgrest.Constants.Operator.Equals, loginDto.Email)
                .Single();

            if (user == null || user.Senha != loginDto.Senha)
            {
                return Unauthorized(new { error = "Email ou senha incorretos" });
            }
            GlobalUser.UserId = user.Id;
            GlobalUser.IsAluno = user.IsAluno;
            GlobalUser.Nome = user.Nome;
            return Ok(new { message = "Login bem-sucedido", user });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Erro ao processar login", details = ex.Message });
        }
    }
}

public class Users : BaseModel
{
    [PrimaryKey]
    [Column("id")]
    public long Id { get; set; }

    [Column("IsAluno")]
    public bool IsAluno { get; set; }

    [Column("Nome")]
    public string Nome { get; set; }

    [Column("Senha")]
    public string Senha { get; set; }

    [Column("Email")]
    public string Email { get; set; }
}


public class LoginDto
{
    public string Email { get; set; }
    public string Senha { get; set; }
}
public class UserDto
{
    public bool IsAluno { get; set; }
    public string Nome { get; set; }
    public string Senha { get; set; }
    public string Email { get; set; }
}
