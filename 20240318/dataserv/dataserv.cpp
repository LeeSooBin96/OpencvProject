#pragma comment(lib, "ws2_32") // socket 사용을위함
#pragma warning(disable:4996) // inet_ntoa를 위함

#include <stdio.h> 
#include <iostream>
#include <vector>
#include <thread>
#include <WinSock2.h> // 소켓쓰기위한 라이브러리

#define BUFFERSIZE 1024 // 수신용버퍼사이즈

using namespace std;

// [1] 클라 데이터 수신하는 함수 ( 사이즈 -> 데이터 순서 수신)
unsigned char* receive(SOCKET clientSock, int* size)
{
	// char* 4바이트로받으면 int형이됨.
	if (recv(clientSock, (char*)size, 4, 0) == SOCKET_ERROR)
	{
		cout << "error" << endl;
		return nullptr;
	}
	// 데이터를 unsigned char 형식으로 받기 = byte
	unsigned char* buffer = new unsigned char[*size];
	if (recv(clientSock, (char*)buffer, *size, 0) == SOCKET_ERROR)
	{
		cout << "error" << endl;
		return nullptr;
	}

	// 받은데이터 리턴하기.
	return buffer;
}

// [2] 클라 접속용 쓰레드
void client(SOCKET clientSock, SOCKADDR_IN clientAddr, vector<thread*>* clientlist)
{
	// 클라 접속정보출력
	cout << "Client connected IP address = " << inet_ntoa(clientAddr.sin_addr) << ":" << ntohs(clientAddr.sin_port) << endl;
	int size; // 데이터사이즈
	wchar_t buffer[BUFFERSIZE] = L"C:\\test\\"; // 저장할 경로
	wchar_t filename[BUFFERSIZE]; // 저장할 파일명변수?
	unsigned char* data = receive(clientSock, &size); // 데이터받기
	//1번째 = 파일명, C# -> utf8 (o) 보냄
	// c++ = unicode로 변환
	MultiByteToWideChar(CP_UTF8, 0, (const char*)data, size, filename, BUFFERSIZE);
	delete data; // 수신데이터 메모리에서 삭제
	wcscat(buffer, filename); // 디렉토리 + 파일명
	data = receive(clientSock, &size); // 데이터 다시 받기 ^ 업로드하는 파일 데이터이다.
	FILE* fp = _wfopen(buffer, L"wb"); // 저장할 파일 객체를 받는다.
	if (fp != NULL)
	{
		fwrite(data, 1, size, fp); // 파일저장
		fclose(fp); // 파일닫기
	}
	else
	{
		// 에러시 출력
		cout << "File open failed" << endl;
	}
	delete data; // 수신데이터 메모리삭제
	char ret[1] = { 1 }; // 송신 데이터 선언 바이트 = 1 보내면 송신완료.
	send(clientSock, ret, 1, 0); // 클라에게 오나료 패킷을 보낸다.
	closesocket(clientSock); // 소켓 닫는다.
	// 접속 종료 출력
	cout << "Client disconnected IP address = " << inet_ntoa(clientAddr.sin_addr) << ":" << ntohs(clientAddr.sin_port) << endl;
	//쓰레드제거
	for (auto ptr = clientlist->begin(); ptr < clientlist->end(); ptr++)
	{
		if ((*ptr)->get_id() == this_thread::get_id())
		{
			clientlist->erase(ptr);
			break;
		}
	}
}

// 실행함수
int main()
{
	vector<thread*> clientlist; // 클라에 접속중인 client list
	WSADATA wsaData; // 소켓정보 데이터 설정
	if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) // 소켓실행
	{
		return 1;
	}
	SOCKET serverSock = socket(PF_INET, SOCK_STREAM, 0); // internet Stream방식으로 소켓 생성
	SOCKADDR_IN addr; // 소켓 주소 설정

	memset(&addr, 0, sizeof(addr)); // 구조체 초기화
	addr.sin_family = AF_INET; 
	addr.sin_addr.s_addr = htonl(INADDR_ANY); // 로컬설정, 다른컴퓨터로하면 htonl()괄호안의 수정요망
	addr.sin_port = htons(9090); // 포트

	if (bind(serverSock, (SOCKADDR*)&addr, sizeof(SOCKADDR_IN)) == SOCKET_ERROR) // [2]bind
	{
		cout << "error" << endl;
		return 1;
	}
	if (listen(serverSock, SOMAXCONN) == SOCKET_ERROR) // 소켓대기
	{
		cout << "error" << endl;
		return 1;
	}
	cout << "Server Start" << endl; // 서버시작


	while (1) // 다중접속을 위함
	{
		int len = sizeof(SOCKADDR_IN); // 접속설정 구조체 사이즈
		SOCKADDR_IN clientAddr; // 접속설정 구조체
		SOCKET clientSock = accept(serverSock, (SOCKADDR*)&clientAddr, &len); // clnt가 접속하면 socket받기
		clientlist.push_back(new thread(client, clientSock, clientAddr, &clientlist)); // 쓰레드실행, 쓰레드 리스트에 넣기
	}

	// 종료시 쓰레드 리스트에 남아 있는 쓰레드를 종료할 때까지 기다림
	if (clientlist.size() > 0)
	{
		for (auto ptr = clientlist.begin(); ptr < clientlist.end(); ptr++)
		{
			(*ptr)->join();
		}
	}
	closesocket(serverSock); // 소켓종료
	WSACleanup(); // 소켓 종료
	return 0;
}