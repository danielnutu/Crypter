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

using Crypter.Common.Contracts.Features.UserSettings.NotificationSettings;
using Crypter.Core.Features.UserSettings.NotificationSettings;
using System;
using System.Threading;
using System.Threading.Tasks;
using EasyMonads;

namespace Crypter.Core.Services.UserSettings
{
   public interface IUserNotificationSettingsService
   {
      Task<Maybe<NotificationSettings>> GetNotificationSettingsAsync(Guid userId, CancellationToken cancellationToken = default);
      Task<Either<UpdateNotificationSettingsError, NotificationSettings>> UpdateNotificationSettingsAsync(Guid userId, NotificationSettings request);
   }

   public class UserNotificationSettingService : IUserNotificationSettingsService
   {
      private readonly DataContext _dataContext;

      public UserNotificationSettingService(DataContext context)
      {
         _dataContext = context;
      }

      public async Task<Maybe<NotificationSettings>> GetNotificationSettingsAsync(Guid userId, CancellationToken cancellationToken = default)
      {
         return await UserNotificationSettingsQueries.GetUserNotificationSettingsAsync(_dataContext, userId, cancellationToken);
      }

      public async Task<Either<UpdateNotificationSettingsError, NotificationSettings>> UpdateNotificationSettingsAsync(Guid userId, NotificationSettings request)
      {
         return await UserNotificationSettingsCommands.UpdateNotificationSettingsAsync(_dataContext, userId, request);
      }
   }
}
