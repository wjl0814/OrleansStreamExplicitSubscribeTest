# OrleansStreamExplicitSubscribeTest
测试多个SiloHost实例能不能都收到订阅的事件

1. `docker-compose -f docker-compose.yml -f docker-compose.staging.windows.yml up --build -d`
2. 执行`volumes`中Orleans初始化脚本
3. docker-compose restart
4. 启动了5个SiloHost实例， 等待10秒后会订阅Stream
5. 用Postman的`post`方法往API 'message'发生message
6. 查看每个SiloHost中的日志， 会打印从Stream中收到的信息
