﻿using AutoMapper;
using Bussiness.Repository.IRepository;
using Common;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bussiness.Repository
{
    public class RoomOrderDetailsRepository : IRoomOrderDetailsRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public RoomOrderDetailsRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<RoomOrderDetailsDTO> Create(RoomOrderDetailsDTO details)
        {
            try
            {
                details.CheckInDate = details.CheckInDate.Date;
                details.CheckOutDate = details.CheckOutDate.Date;
                var roomOrder = _mapper.Map<RoomOrderDetailsDTO, RoomOrderDetails>(details);
                roomOrder.Status = SD.Status_Pending;
                var result = await _db.RoomOrderDetails.AddAsync(roomOrder);
                await _db.SaveChangesAsync();
                return _mapper.Map<RoomOrderDetails, RoomOrderDetailsDTO>(result.Entity);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<IEnumerable<RoomOrderDetailsDTO>> GetAllRoomOrderDetails()
        {
            try
            {
                IEnumerable<RoomOrderDetailsDTO> roomOrders = _mapper.Map<IEnumerable<RoomOrderDetails>, IEnumerable<RoomOrderDetailsDTO>>
                    (_db.RoomOrderDetails.Include(u => u.HotelRoom));

                return roomOrders;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<RoomOrderDetailsDTO> GetRoomOrderDetail(int roomOrderId)
        {
            try
            {
                RoomOrderDetails roomOrder = await _db.RoomOrderDetails
                    .Include(u => u.HotelRoom).ThenInclude(x => x.HotelRoomImages)
                    .FirstOrDefaultAsync(u => u.Id == roomOrderId);

                RoomOrderDetailsDTO roomOrderDetailsDTO = _mapper.Map<RoomOrderDetails, RoomOrderDetailsDTO>(roomOrder);
                roomOrderDetailsDTO.HotelRoomDTO.TotalDays = roomOrderDetailsDTO.CheckOutDate
                    .Subtract(roomOrderDetailsDTO.CheckInDate).Days;

                return roomOrderDetailsDTO;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<bool> IsRoomChecked(int RoomId, DateTime checkInDate, DateTime checkOutDate)
        {
            var status = true;
            var existingBooking = await _db.RoomOrderDetails.Where(x => x.RoomId == RoomId && x.IsPaymentSuccessful &&
            //check if checkin date that user wants does not fall in between any dates for room that is booked
            (checkInDate < x.CheckOutDate && checkInDate.Date > x.CheckInDate
            //check if checkout date that user wants does not fall in between any dates for room that is booked
            ||
             checkOutDate.Date > x.CheckInDate.Date && checkInDate.Date < x.CheckInDate.Date
            )).FirstOrDefaultAsync();
            if(existingBooking != null)
            {
                status = true;
            }
            return status;
        }

        public async Task<RoomOrderDetailsDTO> MarkPaymentSuccessful(int id)
        {
            var data = await _db.RoomOrderDetails.FindAsync(id);
            if (data == null)
            {
                return null;
            }
            if (!data.IsPaymentSuccessful)
            {
                data.IsPaymentSuccessful = true;
                data.Status = SD.Status_Booked;
                var markPaymentSuccessful = _db.RoomOrderDetails.Update(data);
                await _db.SaveChangesAsync();
                return _mapper.Map<RoomOrderDetails, RoomOrderDetailsDTO>(markPaymentSuccessful.Entity);
            }
            return new RoomOrderDetailsDTO();
        }

        public async Task<bool> UpdateOrderStatus(int RoomOrderId, string status)
        {
            try
            {
                var roomOrder = await _db.RoomOrderDetails.FirstOrDefaultAsync(u => u.Id == RoomOrderId);
                if (roomOrder == null)
                {
                    return false;
                }
                roomOrder.Status = status;
                if (status == SD.Status_CheckedIn)
                {
                    roomOrder.ActualCheckInDate = DateTime.Now;
                }
                if (status == SD.Status_CheckedOut_Completed)
                {
                    roomOrder.ActualCheckOutDate = DateTime.Now;
                }
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
