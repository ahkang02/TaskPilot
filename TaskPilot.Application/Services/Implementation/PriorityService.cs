using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Application.Services.Implementation
{
    public class PriorityService : IPriorityService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PriorityService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public bool CheckIfPriorityIsInUse(Priorities priority)
        {
            return _unitOfWork.Tasks.GetAll().Any(t => t.PriorityId == priority.Id);
        }

        public void CreatePriority(Priorities priority)
        {
            _unitOfWork.Priority.Add(priority);
            _unitOfWork.Save();
        }

        public void DeletePriority(Priorities priority)
        {
            _unitOfWork.Priority.Remove(priority);
            _unitOfWork.Save();
        }

        public IEnumerable<Priorities> GetAllPriority()
        {
            return _unitOfWork.Priority.GetAll();
        }

        public Priorities GetPrioritiesById(Guid Id)
        {
            return _unitOfWork.Priority.Get(p => p.Id == Id);
        }

        public Priorities GetPrioritiesByName(string name)
        {
            return _unitOfWork.Priority.Get(p => p.Description == name);
        }

        public void UpdatePriority(Priorities priority)
        {
            _unitOfWork.Priority.Update(priority);
            _unitOfWork.Save();
        }
    }
}
