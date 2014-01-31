package org.waterbear.waterbear_bridge;

import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.app.Activity;
import android.content.Intent;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

public class MainActivity extends Activity implements OnClickListener {

	private TextView log;
	private Button sendTest; 
	private BtInterface bt = null;
	private TCPServer udpServer = null;
	private long lastTime = 0;
	private TextView ipAddress;
	private TextView udpLog;
	private TextView bluetoothLog;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		
        this.log = (TextView) this.findViewById(R.id.log);
        this.udpLog = (TextView) this.findViewById(R.id.udpLog);
        this.bluetoothLog = (TextView) this.findViewById(R.id.bluetoothLog);
        this.ipAddress = (TextView) this.findViewById(R.id.ipaddress);
        String ipAddress = Utils.getIPAddress(true);
        this.ipAddress.setText((ipAddress.length() < 1) ? "Couldn't get IP Address" : "Your IP Address : " + ipAddress);

        this.sendTest = (Button) findViewById(R.id.send_test);
        this.sendTest.setOnClickListener(this);
        
        try{
        	this.bt = new BtInterface(this, handlerStatus, handlerBT);
        	this.bt.connect();
        } catch(Exception e){
        	System.err.println("Bluetooth connect failed - " + e.getMessage());
        }
        
        try{
        	this.udpServer = new TCPServer(handlerUDP);
        	this.udpServer.start();
        } catch(Exception e){
        	System.err.println("UDP server start failed - " + e.getMessage());
        }
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.main, menu);
		return true;
	}
	
	@Override
	protected void onDestroy() {
		super.onDestroy();
		this.bt.close();
		this.udpServer.close();
		try {
			// TODO: wait the thread instead of this ugly thing.
			Thread.sleep(1000);
		} catch (InterruptedException e) {
			e.printStackTrace();
		}
	}

    final Handler handlerStatus = new Handler() {
        public void handleMessage(Message msg) {
            int co = msg.arg1;
            if (co == 0) {
            	log.setText((String) msg.obj);
            } 
        	else if (co == 1) {
            	Exception e = (Exception) msg.obj;
            	StringBuilder errorMsg = 
            			new StringBuilder("Bluetooth Error: \n").append(e.toString());
            	Toast.makeText(
            			MainActivity.this, errorMsg, Toast.LENGTH_LONG)
            			.show();
            }
        	else if (co == 2) {
            	Exception e = (Exception) msg.obj;
            	StringBuilder errorMsg = 
            			new StringBuilder("UDP Error: \n").append(e.toString());
            	Toast.makeText(
            			MainActivity.this, errorMsg, Toast.LENGTH_LONG)
            			.show();
            }

        }
    };
    
	final Handler handlerBT = new Handler() {
        public void handleMessage(Message msg) {
            String data = msg.getData().getString("receivedData");
            
            long t = System.currentTimeMillis();
            if(t - lastTime > 100) { // Pour éviter que les messages soit coupés
				lastTime = System.currentTimeMillis();
			}
            bluetoothLog.setText(data);
        }
    };
    
	final Handler handlerUDP = new Handler() {
        public void handleMessage(Message msg) {
            String data = msg.getData().getString("receivedData");
        	udpLog.setText(data);
        	bt.sendData(data);
        }
	};

	@Override
	public void onClick(View v) {
		if (v == this.sendTest) {
			bt.sendData("Blaise aime les penis ! (Ca c'est malin...)\n");
		}
	}
	
	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
		case R.id.action_settings:
			Intent intent = new Intent(this, SettingsActivity.class);
			startActivity(intent);
			return true;
		default:
			return super.onOptionsItemSelected(item);
		}
	}
}
