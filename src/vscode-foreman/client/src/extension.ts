import * as path from 'path';
import { workspace, ExtensionContext } from 'vscode';

import {
	LanguageClient,
	LanguageClientOptions,
	ServerOptions,
	TransportKind
} from 'vscode-languageclient/node';

let client: LanguageClient;

export function activate(context: ExtensionContext) {
	const serverOptions: ServerOptions = {
		transport: TransportKind.stdio,
		command: 'dotnet',
		args: ['/Users/maule/workspace/lsp-thesequel/src/Foreman.LanguageServer/bin/Debug/net8.0/Foreman.LanguageServer.dll'],
	};

	// Options to control the language client
	const clientOptions: LanguageClientOptions = {
		documentSelector: [{ scheme: 'file', language: 'foreman' }]
	};

	// Create the language client and start the client.
	client = new LanguageClient(
		'foremanLanguageService',
		'Foreman Language Server',
		serverOptions,
		clientOptions
	);

	// Start the client. This will also launch the server
	client.start();
}

export function deactivate(): Thenable<void> | undefined {
	if (!client) {
		return undefined;
	}
	return client.stop();
}