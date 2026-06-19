using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingCenter.Core.Interfaces.Services
{
    public interface INotificationService
    {
        Task SendAsync(string to, string subject, string body);
    }
}
