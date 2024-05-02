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
using Crypter.Common.Enums;
using Crypter.Core.Services;
using Hangfire;
using MediatR;

namespace Crypter.Core.Features.Transfer.Events;

public sealed record SuccessfulTransferDownloadEvent(Guid ItemId, TransferItemType ItemType, TransferUserType UserType, Guid? UserId, bool DeleteItemFromTransferRepository, DateTimeOffset Timestamp) : INotification;

internal sealed class SuccessfulTransferDownloadEventHandler : INotificationHandler<SuccessfulTransferDownloadEvent>
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IHangfireBackgroundService _hangfireBackgroundService;
    
    public SuccessfulTransferDownloadEventHandler(
        IBackgroundJobClient backgroundJobClient,
        IHangfireBackgroundService hangfireBackgroundService)
    {
        _backgroundJobClient = backgroundJobClient;
        _hangfireBackgroundService = hangfireBackgroundService;
    }

    public Task Handle(SuccessfulTransferDownloadEvent notification, CancellationToken cancellationToken)
    {
        _backgroundJobClient.Enqueue(() =>
            _hangfireBackgroundService.DeleteTransferAsync(notification.ItemId, notification.ItemType, notification.UserType, notification.DeleteItemFromTransferRepository));
        
        _backgroundJobClient.Enqueue(() =>
            _hangfireBackgroundService.LogSuccessfulTransferDownloadAsync(notification.ItemId, notification.ItemType, notification.UserId, notification.Timestamp));
        
        return Task.CompletedTask;
    }
}