package org.waterbear.waterbear_bridge;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.Set;
import java.util.UUID;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;
import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.preference.PreferenceManager;
import android.util.Log;

public class BtInterface {
	private Context context;
	private BluetoothDevice device = null;
	private BluetoothSocket socket = null;
	private InputStream receiveStream = null;
	private OutputStream sendStream = null;
	private ReceiverThread receiverThread;

	protected String prefBTName;
	protected Handler hstatus;

	public BtInterface(Context context, Handler hstatus, Handler h) {
		this.context = context;
		
		SharedPreferences sharedPref = 
				PreferenceManager.getDefaultSharedPreferences(
						this.context);
		this.prefBTName = sharedPref.getString("bt_name", "");
		
		Set<BluetoothDevice> setpairedDevices = 
				BluetoothAdapter.getDefaultAdapter().getBondedDevices();
		BluetoothDevice[] pairedDevices = 
				(BluetoothDevice[]) setpairedDevices
				.toArray(new BluetoothDevice[setpairedDevices.size()]);
		
		for(int i=0;i<pairedDevices.length;i++) {
			if(pairedDevices[i].getName().contains(prefBTName)) {
				this.device = pairedDevices[i];
				try {
					this.socket = this.device.createRfcommSocketToServiceRecord(
							UUID.fromString(
									"00001101-0000-1000-8000-00805F9B34FB"));
					this.receiveStream = this.socket.getInputStream();
					this.sendStream = this.socket.getOutputStream();
				} catch (IOException e) {
					this.error(e);
					e.printStackTrace();
				}
				break;
			}
		}

		this.hstatus = hstatus;
		
		this.receiverThread = new ReceiverThread(h);
	}
	
	public void sendData(String data) {
		sendData(data, false);
	}
	
	/** TODO: Maybe deleteScheduledData obsolete */
	public void sendData(String data, boolean deleteScheduledData) {
		try {
			System.out.println("Sending by bluetooth - " + data);
			sendStream.write(data.getBytes());
	        sendStream.flush();
		} catch (IOException e) {
			this.error(e);
			e.printStackTrace();
		}
	}

	public void connect() {
		new Thread() {
			@Override public void run() {
				try {
					socket.connect();
					
					BtInterface.this.info("Bluetooth connected (name: \"" + 
					                       BtInterface.this.prefBTName + "\")\n");
	                
					receiverThread.start();
					
				} catch (IOException e) {
					Log.v("N", "Connection Failed : "+e.getMessage());
					BtInterface.this.error(e);
					e.printStackTrace();
				} catch (NullPointerException e) {
					BtInterface.this.info("Cannot find bluetooth (name: \"" + 
		                                  BtInterface.this.prefBTName + "\")\n");
					e.printStackTrace();
				}
			}
		}.start();
	}

	public void close() {
		try {
			if (socket != null)
				socket.close();
		} catch (IOException e) {
			this.error(e);
			e.printStackTrace();
		}
	}

	public BluetoothDevice getDevice() {
		return device;
	}
	
	private class ReceiverThread extends Thread {
		Handler handler;
		
		ReceiverThread(Handler h) {
			handler = h;
		}
		
		@Override 
		public void run() {
			while(true) {
				try {
					if(receiveStream.available() > 0) {

						byte buffer[] = new byte[100];
						int k = receiveStream.read(buffer, 0, 100);

						if(k > 0) {
							byte rawdata[] = new byte[k];
							for(int i=0;i<k;i++)
								rawdata[i] = buffer[i];
							
							String data = new String(rawdata);

							Message msg = handler.obtainMessage();
							Bundle b = new Bundle();
							b.putString("receivedData", data);
			                msg.setData(b);
			                handler.sendMessage(msg);
						}
					}
				} catch (IOException e) {
					BtInterface.this.error(e);
					e.printStackTrace();
				}
			}
		}
	}
	
	private void error(Exception e) {
		Message msg = hstatus.obtainMessage();
		msg.arg1 = 1;
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
