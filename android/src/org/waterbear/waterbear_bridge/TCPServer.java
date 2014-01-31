package org.waterbear.waterbear_bridge;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.ServerSocket;
import java.net.Socket;

import android.R.bool;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;

public class TCPServer {

	private ServerSocket serverSocket;

	Handler updateConversationHandler;

	Thread serverThread = null;

	Handler handler;

	public static final int SERVERPORT = 6666;

	public TCPServer(Handler handler) {

		this.handler = handler;

		updateConversationHandler = new Handler();

		this.serverThread = new Thread(new ServerThread());

	}

	public void start() {
		this.serverThread.start();
	}

	public void close() {
		try {
			serverSocket.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}

	class ServerThread implements Runnable {

		public void run() {
			Socket socket = null;
			try {
				serverSocket = new ServerSocket(SERVERPORT);
			} catch (IOException e) {
				e.printStackTrace();
			}
			while (!Thread.currentThread().isInterrupted()) {

				try {

					socket = serverSocket.accept();

					CommunicationThread commThread = new CommunicationThread(
							socket);
					new Thread(commThread).start();
					socket.close();
					socket = null;

				} catch (IOException e) {
					e.printStackTrace();
				}
			}
		}
	}

	class CommunicationThread implements Runnable {

		private Socket clientSocket;

		private String data;

		public CommunicationThread(Socket clientSocket) {

			this.clientSocket = clientSocket;

			try {

				// this.input = new BufferedReader(new
				// InputStreamReader(this.clientSocket.getInputStream()));

				int red = -1;
				byte[] buffer = new byte[1]; // a read buffer of 5KiB
				byte[] redData;
				boolean noDataReceived = true;
				StringBuilder clientData = new StringBuilder();
				String redDataText;
				while (noDataReceived
						&& (red = clientSocket.getInputStream().read(buffer)) > -1) {
					redData = new byte[red];
					System.arraycopy(buffer, 0, redData, 0, red);
					redDataText = new String(redData, "UTF-8"); // assumption
																// that client
																// sends data
																// UTF-8 encoded
					System.out.println("message part recieved:" + redDataText);
					if (redDataText.equals("#")) {
						noDataReceived = false;

					} else {
						clientData.append(redDataText);
					}

				}

				this.data = clientData.toString();

			} catch (IOException e) {
				e.printStackTrace();
			}
		}

		public void run() {

			while (!Thread.currentThread().isInterrupted() && this.data != null) {

				Message msg = handler.obtainMessage();

				Bundle b = new Bundle();
				b.putString("receivedData", this.data);
				msg.setData(b);

				handler.sendMessage(msg);

				this.data = null;

			}
		}

	}

}
