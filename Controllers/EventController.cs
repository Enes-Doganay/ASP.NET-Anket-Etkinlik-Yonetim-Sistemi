using Microsoft.AspNetCore.Mvc;
using Basics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
[Authorize]
public class EventController : Controller
{
    private readonly RepositoryContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<EventController> _logger;
    private readonly IEmailSender _emailSender;
    public EventController(RepositoryContext context, UserManager<ApplicationUser> userManager, ILogger<EventController> logger, IEmailSender emailSender)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _emailSender = emailSender;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreatePoll([FromBody] EventPoll eventPoll)
    {
        if (ModelState.IsValid)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            eventPoll.UserId = userId;

            user.EventPolls.Add(eventPoll);

            //_context.Add(eventPoll);
            _context.EventPolls.Add(eventPoll);
            await _context.SaveChangesAsync();

            //tüm kullanıcıları al
            var users = _context.Users.ToList();
            foreach (var u in users)
            {
                var message = new Message(new string[] { u.Email }, "Yeni Anket", $"Yeni bir etkinlik anketi oluşturuldu.Katılmak için lütfen bu linki takip ediniz: https://localhost:7093/Event/EventPollVote/{eventPoll.Id}");
                await _emailSender.SendEmailAsync(message);
            }

            return Json(new { success = true, responseText = "EventPoll başarıyla oluşturuldu." }); // JSON yanıtı döndür
        }
        return Json(new { success = false, responseText = "Bir hata oluştu." }); // Hata durumunda JSON yanıtı döndür
    }

    [HttpPost]
    public async Task<IActionResult> JoinEventPoll(int pollId, int option)
    {
        // Kullanıcıyı ve anketi bul
        Console.WriteLine("Dikkat !!!!!!!!!!! Joined event poll ");

        var userId = _userManager.GetUserId(User);
        Console.WriteLine("!!!!!!!!! User Id burada  " + userId);

        var user = await _userManager.FindByIdAsync(userId);
        Console.WriteLine("!!!!!!!!! User burada  " + user);


        var poll = await _context.EventPolls
            .Include(p => p.Participants)
            .FirstOrDefaultAsync(p => p.Id == pollId);

        Console.WriteLine("!!!!!!!!! dikkat burası poll değişkeni" + poll);
        Console.WriteLine("!!! !!!! ! ! ! Başlangıç countu  " + poll.Participants.Count);

        // Katılımcı zaten ekli değilse ekle
        if (!poll.Participants.Any(p => p.Id == user.Id))
        {
            poll.Participants.Add(user);

            // EntityState.Modified ile durum değişikliklerini takip et
            _context.Entry(poll).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }
        Console.WriteLine("!!! !!!! ! ! ! !Counts " + poll.Participants.Count);
        /*
        if (!poll.Participants.Contains(user)) //bide Participants.id ile dene
        {
            _logger.LogInformation("poll'da yok ");
            Console.WriteLine("pollda yok");
            // Kullanıcıyı Participants koleksiyonuna ekle

            poll.Participants.Add(user);

            _logger.LogInformation("poll'a katılımcı eklendi ");
            Console.WriteLine("polla katılımcı eklendi");

            // Oyu artır
            if (option == 1)
            {
                poll.VotesForTime1++;
            }
            else if (option == 2)
            {
                poll.VotesForTime2++;
            }
        }
        if (!poll.Participants.Any(p => p.Id == userId))
        {
            _logger.LogInformation("poll'da yok ");
            Console.WriteLine("pollda yok");
            // Kullanıcıyı Participants koleksiyonuna ekle


            Console.WriteLine("!!!!!!!!!! eklenmeden önce");
            poll.Participants.Add(user);
            Console.WriteLine("!!!!!!!!!! eklendikten sonra");

            _logger.LogInformation("poll'a katılımcı eklendi ");
            Console.WriteLine("polla katılımcı eklendi");

            // Oyu artır
            if (option == 1)
            {
                poll.VotesForTime1++;
            }
            else if (option == 2)
            {
                poll.VotesForTime2++;
            }
        }
        */
        /*
        if (!poll.Participants.Any(p => p.Id == userId))
        {
            _logger.LogInformation("poll'da yok ");
            Console.WriteLine("pollda yok");
            // Kullanıcıyı Participants koleksiyonuna ekle

            var participant = new Participant { Id = userId };
            poll.Participants.Add(participant);

            _logger.LogInformation("poll'a katılımcı eklendi ");
            Console.WriteLine("polla katılımcı eklendi");

            // Oyu artır
            if (option == 1)
            {
                poll.VotesForTime1++;
            }
            else if (option == 2)
            {
                poll.VotesForTime2++;
            }
        }
        */

        await _context.SaveChangesAsync();
        //return Json(new { success = true, responseText = "Ankete başarıyla katıldınız." });
        return View("JoinEventPollSuccess");
    }

    

    [HttpGet]
    public async Task<IActionResult> EventPollVote(int id)
    {
        // Anketi bul
        var poll = await _context.EventPolls.FindAsync(id);

        if (poll == null)
        {
            return NotFound();
        }

        // Anketin değerlerini ve seçeneklerini içeren bir model oluştur
        var model = new EventPoll
        {
            Id = poll.Id,
            Title = poll.Title,
            Description = poll.Description,
            StartTime1 = poll.StartTime1,
            EndTime1 = poll.EndTime1,
            StartTime2 = poll.StartTime2,
            EndTime2 = poll.EndTime2
        };

        return View(model);
    }


    [HttpGet]
    public async Task<IActionResult> GetUserEvents()
    {
        var userId = _userManager.GetUserId(User);
        var user = await _userManager.Users.Include(u => u.Events).FirstOrDefaultAsync(u => u.Id == userId);

        if (user != null)
        {
            if (user.Events == null || !user.Events.Any())
            {
                _logger.LogInformation("Events özelliği null veya boş.");
            }
            else
            {
                var events = new List<object>();
                //var events = user.Events.Select(e => new { e.Id, e.Title, e.StartTime, e.EndTime });

                foreach (var e in user.Events)
                {
                    events.Add(new { id = e.Id, title = e.Title, start = e.StartTime, end = e.EndTime });
                }

                // Loglama ekleniyor
                _logger.LogInformation("Events: {@Events}", events);

                return Json(events);
            }
        }

        return Json(new List<Event>());
    }

    [HttpGet]
    public async Task<IActionResult> GetUserEventPolls()
    {
        var userId = _userManager.GetUserId(User);
        var user = await _userManager.Users.Include(u => u.EventPolls).FirstOrDefaultAsync(u => u.Id == userId);

        if (user != null)
        {
            if (user.EventPolls == null || !user.EventPolls.Any())
            {
                _logger.LogInformation("EventPolls özelliği null veya boş.");
            }
            else
            {
                var eventPolls = new List<object>();

                foreach (var ep in user.EventPolls)
                {
                    // İlk zaman dilimini temsil eden anketi oluştur
                    eventPolls.Add(new { id = ep.Id, title = ep.Title, start = ep.StartTime1, end = ep.EndTime1 });

                    // İkinci zaman dilimini temsil eden anketi oluştur
                    eventPolls.Add(new { id = ep.Id, title = ep.Title, start = ep.StartTime2, end = ep.EndTime2 });
                }

                // Loglama ekleniyor
                _logger.LogInformation("EventPolls: {@EventPolls}", eventPolls);

                return Json(eventPolls);
            }
        }

        return Json(new List<EventPoll>());
    }
}