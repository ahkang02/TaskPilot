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
    public class StatusService : IStatusService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StatusService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void CreateStatus(Statuses status)
        {
            _unitOfWork.Status.Add(status);
            _unitOfWork.Save();
        }

        public void DeleteStatus(Statuses status)
        {
            _unitOfWork.Status.Remove(status);
            _unitOfWork.Save();
        }

        public IEnumerable<Statuses> GetAllStatuses()
        {
            return _unitOfWork.Status.GetAll();
        }

        public Statuses GetStatusById(Guid Id)
        {
            return _unitOfWork.Status.Get(s => s.Id == Id);
        }

        public Statuses GetStatusByName(string name)
        {
            return _unitOfWork.Status.Get(s => s.Description == name);
        }

        public void UpdateStatus(Statuses status)
        {
            _unitOfWork.Status.Update(status);
            _unitOfWork.Save();
        }

        public bool CheckIfStatusIsInUse(Statuses status)
        {
            return _unitOfWork.Tasks.GetAll().Any(s => s.StatusId == status.Id);
        }
    }
}
