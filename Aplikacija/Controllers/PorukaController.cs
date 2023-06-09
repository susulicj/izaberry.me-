using Microsoft.AspNetCore.Mvc;

namespace Aplikacija.Controllers;

[Authorize]
[ApiController]
[Route("/chat")]

public class PorukaController : ControllerBase
{

    private readonly IzaberryMeDbContext Context;

    public PorukaController(IzaberryMeDbContext Context)
    {
        this.Context = Context;
    }
    [AllowAnonymous]
    [HttpDelete("ObrisiPoruku/{messageId}")]
    public async Task<ActionResult> ObrisiPoruku(int messageId)
    {
        var message = await Context.Poruke.FindAsync(messageId);
        if (message == null)
        {
            return NotFound();
        }

        Context.Poruke.Remove(message);
        await Context.SaveChangesAsync();

        return NoContent();
    }
    [AllowAnonymous]
    [HttpPost("DodajPoruku/{studentID}/{text}")]
    public async Task<ActionResult> DodajPoruku(int studentID, string text)
    {
        var student = await Context.Studenti.Where(s => s.Id == studentID).FirstOrDefaultAsync();
        if (student == null)
        {
            return NotFound();
        }

        var poruka = new Poruka { Student = student, Text = text };

        Context.Poruke.Add(poruka);
        await Context.SaveChangesAsync();

        return Created($"{poruka.Id}", poruka);
    }
     [AllowAnonymous]
    [HttpGet("VratiPorukeIzChata/{chatId}")]
    public async Task<ActionResult> VratiPorukeIzChata(int chatId)
    {
        var chat = await Context.Chats
            .Include(c => c.Poruke)
            .ThenInclude(c=> c.Student)
            .FirstOrDefaultAsync(c => c.Id == chatId);
        if (chat == null)
        {
            return NotFound();
        }

        var poruke = chat.Poruke;
        return Ok(poruke);
    }
    [AllowAnonymous]
    [HttpGet("VratiNeprocitanePorukeStudenta/{studentId}")]
    public async Task<ActionResult<IEnumerable<Poruka>>> VratiNeprocitanePorukeStudenta(int studentId)
    {
        var poruke = await Context.Poruke
            .Include(p => p.Student)
            .Include(p => p.chat)
            .ThenInclude(p=> p.StudentPosiljaoc)
            .Where(p => p.chat.StudentPrimaoc.Id == studentId && !p.procitana)
            .ToListAsync();

        return Ok(poruke);
    }




}
