package org.waterbear.waterbear_bridge;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.SocketException;

import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.preference.PreferenceManager;


public class UDPServer {
	private ReceiverThread receiverThread;
	private DatagramSocket serverSocket = null;
	protected volatile Boolean run = true;
	protected Handler hstatus;
	protected Context context;
	
	public UDPServer(Context context, Handler hstatus, Handler handler) {
		this.context = context;
		this.hstatus = hstatus;
		this.receiverThread = new ReceiverThread(handler);
	}
	
	public void start() {
		this.receiverThread.start();
	}
	
	private class ReceiverThread extends Thread {
		Handler handler;
		
		ReceiverThread(Handler h) {
			handler = h;
		}
		
		@Override 
		public void run() {
			Integer port;
			SharedPreferences sharedPref = 
					PreferenceManager.getDefaultSharedPreferences(
							UDPServer.this.context);
			String prefPort = sharedPref.getString("udp_port", "");
			try {
			port = Integer.parseInt(prefPort);
			} catch (NumberFormatException e) {
				port = 6666;
			}
			
			try {
				serverSocket = new DatagramSocket(port);
			} catch (SocketException e) {
				UDPServer.this.error(e);
				e.printStackTrace();
			}
			UDPServer.this.info("UDP server listening (port:  " + port + ")\n");
			
	    	DatagramPacket receivePacket;
	    	
			while(run) {
				try {
					System.out.println("fuck you nigger, fuck you");
					byte[] receiveData = new byte[4096];
					receivePacket = new DatagramPacket(
							receiveData, receiveData.length);
					
					serverSocket.receive(receivePacket);					
					String data = new String(receiveData);
					System.out.println("Received by UDP - " + data);

					Message msg = handler.obtainMessage();
					Bundle b = new Bundle();
					b.putString("receivedData", data);
					msg.setData(b);
					handler.sendMessage(msg);
				} catch (IOException e) {
					UDPServer.this.error(e);
					e.printStackTrace();
				}
			}
			serverSocket.close();
		}
	}
	
	public void close() {
		run = false;
	}
	
	private void error(Exception e) {
		Message msg = hstatus.obtainMessage();
		msg.arg1 = 2;
		msg.obj = e;
		hstatus.sendMessage(msg);
	}
	
	private void info(String s) {
		Message msg = hstatus.obtainMessage();
		msg.arg1 = 0;
		msg.obj = new String(s);
		hstatus.sendMessage(msg);
	}
}
