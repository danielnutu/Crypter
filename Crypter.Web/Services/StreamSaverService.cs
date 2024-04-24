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
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Crypter.Web.Services;

public interface IStreamSaverService
{
    Task InitializeAsync();
    Task SaveFileAsync(Stream stream, string fileName, string mimeType, long? size);
}

public class StreamSaverService(IJSRuntime jsRuntime) : IStreamSaverService
{
    private IJSInProcessObjectReference? _moduleReference;

    public async Task InitializeAsync()
    {
        if (OperatingSystem.IsBrowser())
        {
            Console.WriteLine("initializing StreamSaverService");
            await JSHost.ImportAsync("streamSaver", "../js/dist/streamSaver/streamSaver.bundle.js");
            _moduleReference = await jsRuntime.InvokeAsync<IJSInProcessObjectReference>("import", "../js/dist/streamSaver/streamSaver.bundle.js");
            await _moduleReference.InvokeVoidAsync("init");
        }
    }

    public async Task SaveFileAsync(Stream stream, string fileName, string mimeType, long? size)
    {
        using DotNetStreamReference streamReference = new DotNetStreamReference(stream, leaveOpen: false);
        await _moduleReference!.InvokeVoidAsync("saveFile", streamReference, fileName, mimeType, size);
    }
}
