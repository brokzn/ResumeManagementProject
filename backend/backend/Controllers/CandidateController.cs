using AutoMapper;
using backend.Core.Context;
using backend.Core.Dtos.Company.Candidate;
using backend.Core.Dtos.Company.Company;
using backend.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private ApplicationDbContext _context { get; }
        private IMapper _mapper { get; }

        public CandidateController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        //CRUD(Create,Read,Update,Delete)

        //Create
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> CreateCandidate([FromForm] CandidateCreateDto dto, IFormFile pdfFile)
        {
            //В начале мы сохраняем PDF-файл на сервер
            //Далее мы сохраняем URL в нашу сущность
            var fiveMegaByte = 5 * 1024 * 1024;
            var pdfMimeType = "application/pdf";

            if (pdfFile.Length > fiveMegaByte || pdfFile.ContentType != pdfMimeType)
            {
                return BadRequest("Файл не поддерживается");
            }

            var resumeUrl = Guid.NewGuid().ToString() + ".pdf";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "documents", "pdfs", resumeUrl);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await pdfFile.CopyToAsync(stream);
            }

            var newCandidate = _mapper.Map<Candidate>(dto);
            newCandidate.ResumeUrl = resumeUrl;
            await _context.Candidates.AddAsync(newCandidate);
            await _context.SaveChangesAsync();

            return Ok("Кандидат успешно добавлен");
        }

        //Read
        [HttpGet]
        [Route("Get")]
        public async Task<ActionResult<IEnumerable<CandidateGetDto>>> GetCandidate()
        {
            var candidates = await _context.Candidates.Include(c => c.Job).ToListAsync();
            var convertedCandidate = _mapper.Map<IEnumerable<CandidateGetDto>>(candidates);

            return Ok(convertedCandidate);
        }

        //Read (Download PDF-file)
        [HttpGet]
        [Route("download/{url}")]
        public IActionResult DownloadPdfFile(string url)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "documents", "pdfs", url);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Файл не обнаружен");
            }

            var pdfBytes = System.IO.File.ReadAllBytes(filePath);
            var file = File(pdfBytes, "application/pdf", url);
            return file;
        }

        //Read (Get Company By ID)

        //Update

        //Delete

    }
}
