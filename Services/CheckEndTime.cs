using System.Diagnostics;
using Basics.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
public class CheckEndTime : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    public CheckEndTime(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // İşlevi çağır ve gerekli kontrolleri yap
            await DoJob();

            // 10 saniyede bir çalıştır
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task DoJob()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var _context = scope.ServiceProvider.GetRequiredService<RepositoryContext>(); // YourDbContext, veritabanı bağlantınızı temsil eder
            var _emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>(); // YourDbContext, veritabanı bağlantınızı temsil eder
            var _userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(); // YourDbContext, veritabanı bağlantınızı temsil eder
            Console.WriteLine("çalıştı");

            var expiredPolls = _context.EventPolls
                .Include(p => p.Participants)
                .Where(p => p.PollEndTime <= DateTime.Now);



            Console.WriteLine("Tamamlanan anketler " + expiredPolls.Count());

            foreach (var poll in expiredPolls)
            {
                Console.WriteLine("Bu bir Anket Id " + poll.Id);
                DateTime startTime, endTime;

                if (poll.VotesForTime1 > poll.VotesForTime2)
                {
                    startTime = poll.StartTime1;
                    endTime = poll.EndTime1;
                }
                else if (poll.VotesForTime1 < poll.VotesForTime2)
                {
                    startTime = poll.StartTime2;
                    endTime = poll.EndTime2;
                }
                else
                {
                    startTime = poll.StartTime1;
                    endTime = poll.EndTime1;
                }

                var e = new Event
                {
                    Title = poll.Title,
                    Description = poll.Description,
                    Location = poll.Location,
                    StartTime = startTime,
                    EndTime = endTime
                };

                _context.Events.Add(e);
                await _context.SaveChangesAsync();
                
                Console.WriteLine("Poll " + poll);
                Console.WriteLine("Poll katılımcıları " + poll.Participants.Count);

                foreach (var participant in poll.Participants)
                {
                    var user = await _userManager.FindByIdAsync(participant.Id);
                    user.Events.Add(e);
                    user.EventPolls.Remove(poll);

                    // Etkinlik oluşturulduğuna dair bir e-posta oluştur
                    var message = new Message(new string[] { user.Email },
                        "Yeni Etkinlik Oluşturuldu",
                        $"Merhaba {user.UserName},\n\n'{e.Title}' başlıklı yeni bir etkinlik oluşturuldu. Etkinlik detayları aşağıdadır:\n\nBaşlık: {e.Title}\nAçıklama: {e.Description}\nKonum: {e.Location}\nBaşlangıç Zamanı: {e.StartTime}\nBitiş Zamanı: {e.EndTime}\n\nKatılımınızı bekliyoruz!");

                    // E-postayı gönder
                    _emailSender.SendEmail(message);
                }
                _context.EventPolls.Remove(poll);
                await _context.SaveChangesAsync();
            }
            Console.WriteLine("işlem tamamlandı");
        }
    }
}