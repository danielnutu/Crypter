﻿/*
 * Copyright (C) 2022 Crypter File Transfer
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

using Crypter.ClientServices.Interfaces;
using Crypter.ClientServices.Transfer.Handlers.Base;
using Crypter.ClientServices.Transfer.Models;
using Crypter.Common.Enums;
using Crypter.Common.Monads;
using Crypter.Common.Primitives;
using Crypter.Contracts.Features.Transfer;
using Crypter.CryptoLib.Crypto;
using Crypter.CryptoLib.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Crypter.ClientServices.Transfer.Handlers
{
   public class UploadMessageHandler : UploadHandler
   {
      private string _messageSubject;
      private string _messageBody;
      private int _expirationHours;

      public UploadMessageHandler(ICrypterApiService crypterApiService, ISimpleEncryptionService simpleEncryptionService, ISimpleSignatureService simpleSignatureService, UploadSettings uploadSettings)
         : base(crypterApiService, simpleEncryptionService, simpleSignatureService, uploadSettings)
      { }

      internal void SetTransferInfo(string messageSubject, string messageBody, int expirationHours)
      {
         _messageSubject = messageSubject;
         _messageBody = messageBody;
         _expirationHours = expirationHours;
      }

      public async Task<Either<UploadTransferError, UploadHandlerResponse>> UploadAsync(Maybe<Func<Task>> invokeBeforeSigning, Maybe<Func<Task>> invokeBeforeUploading)
      {
         if (_recipientUsername.IsNone)
         {
            CreateEphemeralRecipientKeys();
         }

         if (!_senderDefined)
         {
            CreateEphemeralSenderKeys();
         }

         PEMString senderDiffieHellmanPrivateKey = _senderDiffieHellmanPrivateKey.Match(
            () => throw new Exception("Missing sender Diffie Hellman private key"),
            x => x);

         PEMString recipientDiffieHellmanPublicKey = _recipientDiffieHellmanPublicKey.Match(
            () => throw new Exception("Missing recipient Diffie Hellman private key"),
            x => x);

         PEMString senderDigitalSignaturePrivateKey = _senderDigitalSignaturePrivateKey.Match(
            () => throw new Exception("Missing recipient Digital Signature private key"),
            x => x);

         (byte[] sendKey, byte[] serverKey) = DeriveSymmetricKeys(senderDiffieHellmanPrivateKey, recipientDiffieHellmanPublicKey);
         byte[] initializationVector = AES.GenerateIV();

         byte[] ciphertext = _simpleEncryptionService.Encrypt(sendKey, initializationVector, _messageBody);
         await invokeBeforeSigning.IfSomeAsync(async x => await x.Invoke());
         byte[] signature = _simpleSignatureService.Sign(senderDigitalSignaturePrivateKey, _messageBody);
         await invokeBeforeUploading.IfSomeAsync(async x => await x.Invoke());

         string encodedInitializationVector = Convert.ToBase64String(initializationVector);

         List<string> encodedCipherText = new List<string> { Convert.ToBase64String(ciphertext) };

         string encodedSignature = Convert.ToBase64String(signature);

         string encodedECDSASenderKey = _senderDigitalSignaturePublicKey.Match(
            () => throw new Exception("Missing sender Digital Signature public key"),
            x =>
            {
               return Convert.ToBase64String(
                  Encoding.UTF8.GetBytes(x.Value));
            });

         string encodedECDHSenderKey = _senderDiffieHellmanPublicKey.Match(
            () => throw new Exception("Missing sender Diffie Hellman public key"),
            x =>
            {
               return Convert.ToBase64String(
                  Encoding.UTF8.GetBytes(x.Value));
            });

         string encodedServerKey = Convert.ToBase64String(serverKey);

         var request = new UploadMessageTransferRequest(_messageSubject, encodedInitializationVector, encodedCipherText, encodedSignature, encodedECDSASenderKey, encodedECDHSenderKey, encodedServerKey, _expirationHours);
         var response = await _recipientUsername.Match(
            () => _crypterApiService.UploadMessageTransferAsync(request, _senderDefined),
            x => _crypterApiService.SendUserMessageTransferAsync(x, request, _senderDefined));

         return response.Map(x => new UploadHandlerResponse(x.Id, _expirationHours, TransferItemType.Message, x.UserType, _recipientDiffieHellmanPrivateKey));
      }
   }
}
