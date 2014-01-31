#define MAX_SPEED		255
#define MIN_SPEED		40
#define ACCELERATION_DELAY	10

// #define ENABLE_A		5
// #define ENABLE_B		6
#define INA_1			5
#define INA_2			6
#define INB_1			9
#define INB_2			10

#define DIR_STOP		0
#define DIR_FORWARD		1
#define DIR_BACKWARD		2
#define DIR_LEFT		3
#define DIR_RIGHT		4

//--------------------------------------------------------
// TYPE DEFINITION
//--------------------------------------------------------
struct Direction
{
  int in[4];
};

struct DirectionMap
{
  char      key;
  Direction dir;
};

typedef struct {
  Direction	dirSpeed;
} t_motor;

typedef struct {
  char flag;
  uint8_t pin;
  uint8_t value;
} 
__attribute__((__packed__))data_packet_t;

//--------------------------------------------------------
// INIT VARIABLE
//--------------------------------------------------------
Direction forward  = { { 1, 0, 1, 0 } };
Direction backward = { { 0, 1, 0, 1 } };
Direction left     = { { 0, 1, 1, 0 } };
Direction right    = { { 1, 0, 0, 1 } };
Direction stand    = { { 0, 0, 0, 0 } };

DirectionMap dirMap[] =
{
  { 's', stand },
  { 'a', forward },
  { 'r', backward },
  { 'g', left },
  { 'd', right },
  { 0, NULL }
};

Direction *dir;

t_motor motor;

data_packet_t data;

//--------------------------------------------------------
// MAP FUNCTION
//--------------------------------------------------------
struct Direction *getDir(char c)
{
  int i = 0;
  while (dirMap[i].key != 0)
  {
    if (dirMap[i].key == c)
      return &dirMap[i].dir;
    ++i;
  }
  return NULL;
}

//--------------------------------------------------------
// ARDUINO FUNCTIONS
//--------------------------------------------------------
void setup() {
  Serial.begin(9600);
  // pinMode(ENABLE_A, OUTPUT);
  // pinMode(ENABLE_B, OUTPUT);
  pinMode(INA_1, OUTPUT);
  pinMode(INA_2, OUTPUT);
  pinMode(INB_1, OUTPUT);
  pinMode(INB_2, OUTPUT);

  // analogWrite(ENABLE_A, 255);
  // analogWrite(ENABLE_B, 255);
}

void loop() {
  //int			buffer;
  struct Direction	*dir;

  unsigned long buffer_size = sizeof(data_packet_t);
  char buffer[buffer_size];

  if (Serial.available())
  {
    if (Serial.readBytes(buffer, buffer_size) > 0)
    { 
      memcpy(&data, buffer, buffer_size);

    //if (Serial.available() > 0) {
      //buffer = Serial.read();
      
      Serial.print("pin: ");
      Serial.print(data.pin);
      Serial.print(" value: ");
      Serial.println(data.value);
      
      if ((dir = getDir((char) data.value)) != NULL)
	startMotor(*dir);
    }
  }
  run();
}

//--------------------------------------------------------
// MOTOR CONTROL
//--------------------------------------------------------
void run() {
  acceleration();
}

// Compare two dir, return true if egual
bool isDirEqual(struct Direction &dir1, struct Direction &dir2) {
  bool isDifferent = false;

  for (int i = 0; i < 4; ++i)
    isDifferent |= (dir1.in[i] != dir2.in[i]);
  return !isDifferent;
}

void startMotor(struct Direction &dir) {
  // Progressif start if we want to go forward
  if (isDirEqual(dir, forward))
    motor.dirSpeed = dir;

  // Keep the speed if one of the new direction is the same than before
  else
    {
      for (int i = 0; i < 4; ++i) {
	if (!(dir.in[i] && motor.dirSpeed.in[i]))
	  motor.dirSpeed.in[i] = dir.in[i];
      }
    }

  // Init with the MIN_SPEED the active pins
  for (int i = 0; i < 4; ++i) {
    // if (!(dir.in[i] && motor.dirSpeed.in[i]))
    //   motor.dirSpeed.in[i] = dir.in[i];
    if (motor.dirSpeed.in[i] == 1)
      motor.dirSpeed.in[i] = MIN_SPEED + 1;
  }
}

void acceleration() {
  // The pin(Direction.in[*]) which have a value greater than 0 will be
  // incremented, the others will keep the value 0
  for (int i = 0; i < 4; ++i) {
    if (motor.dirSpeed.in[i] > 0 && motor.dirSpeed.in[i] < MAX_SPEED)
      (motor.dirSpeed.in[i])++;
  }
  move(motor.dirSpeed);
  delay(ACCELERATION_DELAY);
}

void move(struct Direction &dir)
{
  analogWrite(INA_1, dir.in[0]);
  analogWrite(INA_2, dir.in[1]);
  analogWrite(INB_1, dir.in[2]);
  analogWrite(INB_2, dir.in[3]);
}
