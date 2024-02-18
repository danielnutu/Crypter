﻿/*
 * Copyright (C) 2023 Crypter File Transfer
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crypter.Common.Contracts.Features.Transfer;
using Crypter.Core.Services;
using Crypter.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crypter.Core.Features.Transfer.Queries;

public sealed record UserSentFilesQuery(Guid UserId)
    : IRequest<IEnumerable<UserSentFileDTO>>;

internal class UserSentFilesQueryHandler
    : IRequestHandler<UserSentFilesQuery, IEnumerable<UserSentFileDTO>>
{
    private readonly DataContext _dataContext;
    private readonly IHashIdService _hashIdService;

    public UserSentFilesQueryHandler(DataContext dataContext, IHashIdService hashIdService)
    {
        _dataContext = dataContext;
        _hashIdService = hashIdService;
    }

    public async Task<IEnumerable<UserSentFileDTO>> Handle(UserSentFilesQuery request, CancellationToken cancellationToken)
    {
        var sentFiles = await _dataContext.UserFileTransfers
            .Where(x => x.SenderId == request.UserId)
            .OrderBy(x => x.Expiration)
            .Select(x => new { x.Id, x.FileName, x.Recipient!.Username, x.Recipient!.Profile!.Alias, x.Expiration })
            .ToListAsync(cancellationToken);

        return sentFiles
            .Select(x =>
                new UserSentFileDTO(_hashIdService.Encode(x.Id), x.FileName, x.Username, x.Alias, x.Expiration));
    }
}
