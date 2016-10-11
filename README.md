# m-duel-project / backend-batch
이 repo는 2016년 10월 12~14일간 예정된 m-duel 프로젝트에 대한 backend batch 작업을 수행하는 repo

### webjob 또는 functinos
scheduled 이나 webhook, trigger에 의해 backend task를 수행할 수 있는 webjob과 functions를 활용해 아래의 작업을 backend에서 수행  
- batch prediction에 의해 분석될 데이터는 admin web으로부터 자동 생성 되는것으로 가정
- admin web은 blob storage의 input container에 JSON 형식 파일을 주기적으로 업로드 한다고 가정
- backend process인 webjob이나 functions는 blob storage에서 파일을 읽는 작업 수행
	1. 주기적으로 polling
	2. webhook
	3. blob trigger
- 읽은 파일을 azure ml batch를 통해 결과로 받음
- 결과를 JSON으로 생성해 blob output container에 기록

위의 과정을 수행하는 webjob 또는 functions를 구현