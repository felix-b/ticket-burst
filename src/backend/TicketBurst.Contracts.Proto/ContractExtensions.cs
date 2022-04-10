using System.Collections.Immutable;
using Google.Protobuf.WellKnownTypes;
using TicketBurst.Reservation.Integrations.SimpleSharding;

namespace TicketBurst.Contracts.Proto;

public static class ContractExtensions
{
    public static TryReserveSeatsRequest ToProto(this SeatReservationRequestContract source)
    {
        var result = new TryReserveSeatsRequest() {
            Key = new ActorKey {
                EventId = source.EventId,
                AreaId = source.HallAreaId
            }
        };
        result.SeatIds.AddRange(source.SeatIds);
        return result;
    }
    
    public static SeatReservationRequestContract FromProto(this TryReserveSeatsRequest source)
    {
        return new SeatReservationRequestContract(
            eventId:  source.Key.EventId,
            hallAreaId: source.Key.AreaId,
            seatIds: source.SeatIds.ToArray(),
            clientContext: null
        );
    }

    public static TryReserveSeatsResponse ToProto(this SeatReservationReplyContract source)
    {
        var result = new TryReserveSeatsResponse() {
            Request = source.Request.ToProto(),
            Success = source.Success,
            CheckoutToken = source.CheckoutToken,
        };

        if (source.ReservationExpiryUtc.HasValue)
        {
            result.ReservationExpiryUtc = Timestamp.FromDateTime(source.ReservationExpiryUtc.Value);
        }

        if (source.ErrorCode != null)
        {
            result.ErrorCode = source.ErrorCode;
        }
        
        return result;
    }
    
    public static SeatReservationReplyContract FromProto(this TryReserveSeatsResponse source)
    {
        return new SeatReservationReplyContract(
            Request: source.Request.FromProto(),
            Success: source.Success,
            CheckoutToken: source.CheckoutToken,
            ReservationExpiryUtc: source.ReservationExpiryUtc.ToDateTime(),
            ErrorCode: source.ErrorCode
        );
    }

    public static EventAreaUpdateNotificationContract FromProto(this GetUpdateNotificationResponse source)
    {
        var notification = source.Notification!;
        return new EventAreaUpdateNotificationContract(
            Id: notification.Id,
            SequenceNo: notification.SequenceNo,
            PublishedAtUtc: notification.PublishedAtUtc.ToDateTime(),
            EventId: notification.EventId,
            HallAreaId: notification.HallAreaId,
            TotalCapacity: notification.TotalCapacity,
            AvailableCapacity: notification.AvailableCapacity,
            StatusBySeatId: notification.StatusBySeatId
                .Select(kvp => new KeyValuePair<string, SeatStatus>(kvp.Key, (SeatStatus)kvp.Value))
                .ToImmutableDictionary()
        );
    }

    public static GetUpdateNotificationResponse ToProto(this EventAreaUpdateNotificationContract source)
    {
        var response = new GetUpdateNotificationResponse {
            Notification = new EventAreaUpdateNotificationMessage {
                Id = source.Id,
                SequenceNo = source.SequenceNo,
                PublishedAtUtc = source.PublishedAtUtc.ToTimestamp(),
                EventId = source.EventId,
                HallAreaId = source.HallAreaId,
                TotalCapacity = source.TotalCapacity,
                AvailableCapacity = source.AvailableCapacity
            }
        };

        foreach (var kvp in source.StatusBySeatId)
        {
            response.Notification.StatusBySeatId.Add(kvp.Key, (int)kvp.Value);
        }

        return response;
    }

    public static FindEffectiveJournalRecordByIdResponse ToProto(this ReservationJournalRecord? source)
    {
        return new FindEffectiveJournalRecordByIdResponse {
            Record = source != null
                ? GetRecordMessage()
                : null
        };
        
        ReservationJournalRecordMessage GetRecordMessage()
        {
            var message = new ReservationJournalRecordMessage {
                Id = source.Id,
                CreatedAtUtc = source.CreatedAtUtc.ToTimestamp(),
                EventId = source.EventId,
                HallAreaId = source.HallAreaId,
                HallSeatingMapId = source.HallSeatingMapId,
                SequenceNo = source.SequenceNo,
                ReservationAction = (int)source.Action,
                ResultSeatStatus = (int)source.ResultStatus,
            };

            message.SeatIds.AddRange(source.SeatIds);
            if (source.OrderNumber.HasValue)
            {
                message.OrderNumber = source.OrderNumber.Value;
            }

            return message;
        }
    }

    public static ReservationJournalRecord? FromProto(this FindEffectiveJournalRecordByIdResponse source)
    {
        return source.Record != null
            ? new ReservationJournalRecord(
                Id: source.Record.Id, 
                CreatedAtUtc: source.Record.CreatedAtUtc.ToDateTime(), 
                EventId: source.Record.EventId, 
                HallAreaId: source.Record.HallAreaId, 
                HallSeatingMapId: source.Record.HallSeatingMapId, 
                SequenceNo: source.Record.SequenceNo, 
                SeatIds: source.Record.SeatIds.ToImmutableList(), 
                Action: (ReservationAction)source.Record.ReservationAction, 
                ResultStatus: (SeatStatus)source.Record.ResultSeatStatus, 
                OrderNumber : source.Record.HasOrderNumber 
                    ? source.Record.OrderNumber
                    : null
            )
            : null;
    }
}

