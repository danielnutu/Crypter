/*
 * Copyright (C) 2024 Crypter File Transfer
 *
 * This file is part of the Crypter file transfer project.
 *
 * Crypter is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * The Crypter source code is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * You can be released from the requirements of the aforementioned license
 * by purchasing a commercial license. Buying such a license is mandatory
 * as soon as you develop commercial activities involving the Crypter source
 * code without disclosing the source code of your own applications.
 *
 * Contact the current copyright holder to discuss commercial license options.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Crypter.Common.Contracts.Features.Transfer;
using Crypter.Common.Enums;
using Crypter.DataAccess;
using Crypter.DataAccess.Entities;
using Crypter.DataAccess.Entities.JsonTypes.EventLogAdditionalData;
using MediatR;
using Unit = EasyMonads.Unit;

namespace Crypter.Core.Features.EventLog.Commands;

public sealed record LogFailedMultipartTransferAbandonCommand(string HashId, TransferItemType ItemType, Guid SenderId, AbandonMultipartFileTransferError Reason, DateTimeOffset Timestamp)
    : IRequest<Unit>;

internal sealed class LogFailedMultipartTransferAbandonCommandHandler
    : IRequestHandler<LogFailedMultipartTransferAbandonCommand, Unit>
{
    private readonly DataContext _dataContext;

    public LogFailedMultipartTransferAbandonCommandHandler(DataContext dataContext)
    {
        _dataContext = dataContext;
    }


    public async Task<Unit> Handle(LogFailedMultipartTransferAbandonCommand request, CancellationToken cancellationToken)
    {
        FailedMultipartTransferAbandonAdditionalData additionalData = new FailedMultipartTransferAbandonAdditionalData(request.HashId, request.ItemType, request.SenderId, request.Reason);
        EventLogEntity logEntity = EventLogEntity.Create(EventLogType.TransferMultipartAbandonFailure, additionalData, request.Timestamp);

        _dataContext.EventLogs.Add(logEntity);
        await _dataContext.SaveChangesAsync(CancellationToken.None);

        return Unit.Default;
    }
}
