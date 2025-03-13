using Microsoft.AspNetCore.Mvc;
using QRCoder;
using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class ClassesController : ControllerBase
{
    private readonly Supabase.Client _supabase;

    public ClassesController(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    [HttpPost("CreateClass")]
    public async Task<IActionResult> CreateClass([FromBody] CreateClassDto dto)
    {
        try
        {
            string classId = Guid.NewGuid().ToString();
            string qrCode = GenerateQrCode(classId);

            var newClass = new Classes
            {
                ClassId = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0),
                Duracao = dto.Duracao,
                QrCode = qrCode
            };

            var response = await _supabase.From<Classes>().Insert(newClass);
            var createdClass = response.Models.FirstOrDefault();

            return Ok(new { message = "Classe criada!", data = createdClass });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Erro ao criar classe", details = ex.Message });
        }
    }

    [HttpPost("RegisterPresence")]
    public async Task<IActionResult> RegisterPresence([FromBody] RegisterPresenceDto dto)
    {
        try
        {
            var classExists = await _supabase
                .From<Classes>()
                .Filter("ClassId", Supabase.Postgrest.Constants.Operator.Equals, dto.ClassId)
                .Single();

            if (classExists == null)
                return NotFound(new { error = "Classe não encontrada" });

            var newHistory = new ClassesHistory
            {
                ClassesHistoryId = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0),
                ClassId = dto.ClassId,
                AlunoId = GlobalUser.UserId // Pega o ID do usuário logado
            };

            var response = await _supabase.From<ClassesHistory>().Insert(newHistory);
            return Ok(new { message = "Presença registrada!", data = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Erro ao registrar presença", details = ex.Message });
        }
    }


    private string GenerateQrCode(string classId)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(classId, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrCodeBytes = qrCode.GetGraphic(20);
        return Convert.ToBase64String(qrCodeBytes);
    }
}

public class CreateClassDto
{
    public int Duracao { get; set; }
}

public class RegisterPresenceDto
{
    public string ClassId { get; set; }
}
public class Classes : BaseModel
{
    [PrimaryKey]
    [Column("ClassId")]
    public long ClassId { get; set; }

    [Column("Duracao")]
    public int Duracao { get; set; } // duração em minutos

    [Column("QrCode")]
    public string QrCode { get; set; }
}

public class ClassesHistory : BaseModel
{
    [PrimaryKey]
    [Column("ClassesHistoryId")]
    public long ClassesHistoryId { get; set; }

    [Column("ClassId")]
    public string ClassId { get; set; }

    [Column("AlunoId")]
    public long AlunoId { get; set; }
}
