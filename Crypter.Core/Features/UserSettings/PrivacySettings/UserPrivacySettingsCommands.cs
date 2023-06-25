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

using Crypter.Common.Contracts.Features.UserSettings.PrivacySettings;
using Crypter.Common.Monads;
using Crypter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Contracts = Crypter.Common.Contracts.Features.UserSettings.PrivacySettings;

namespace Crypter.Core.Features.UserSettings.PrivacySettings
{
   internal static class UserPrivacySettingsCommands
   {
      internal static async Task<Either<Contracts.SetPrivacySettingsError, Contracts.PrivacySettings>> SetPrivacySettingsAsync(DataContext dataContext, Guid userId, Contracts.PrivacySettings request)
      {
         var userPrivacySettings = await dataContext.UserPrivacySettings
            .FirstOrDefaultAsync(x => x.Owner == userId);

         if (userPrivacySettings is null)
         {
            var newPrivacySettings = new UserPrivacySettingEntity(userId, request.AllowKeyExchangeRequests, request.VisibilityLevel, request.FileTransferPermission, request.MessageTransferPermission);
            dataContext.UserPrivacySettings.Add(newPrivacySettings);
         }
         else
         {
            userPrivacySettings.AllowKeyExchangeRequests = request.AllowKeyExchangeRequests;
            userPrivacySettings.Visibility = request.VisibilityLevel;
            userPrivacySettings.ReceiveFiles = request.FileTransferPermission;
            userPrivacySettings.ReceiveMessages = request.MessageTransferPermission;
         }

         await dataContext.SaveChangesAsync();
         return await UserPrivacySettingsQueries.GetPrivacySettingsAsync(dataContext, userId)
            .ToEitherAsync(SetPrivacySettingsError.UnknownError);
      }
   }
}