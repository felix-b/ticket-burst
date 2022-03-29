import React from 'react'
import { Box, Center, Stack, Text, Button, Tag, TagLabel, LinkOverlay } from "@chakra-ui/react"
import { EventCardProps } from '../contracts/props'
import Link from 'next/link'

export const HeroEventCard = (props: EventCardProps) => {
    const {
        eventId,
        dateText,
        eventNameText,
        priceText,
        seatsLeftBadgeText,
        showNameText,
        venueText
    } = props

    return (
        <Stack w="100%" spacing={0} alignContent='center' color='white' justifyItems={'center'}>
            <Text textAlign={'center'} fontSize='3xl' fontWeight={'bold'} >{dateText}</Text>
            <Text textAlign={'center'} fontSize='5xl' fontWeight={'bold'} style={{lineHeight: 0.9, paddingBottom: '5px'}}>{eventNameText}&nbsp;</Text>
            <Text textAlign={'center'} fontSize='xl' fontWeight={'bold'} color='orange.300' pt="20px">{priceText}</Text>
            <Box>
                <Center>
                    <Link href={`/event?id=${eventId}`}>
                        <Button mt='5px' mb='25px' colorScheme='green' size='lg' fontWeight={'bold'} fontSize='2xl' minW='300px' h='60px'>
                            Grab Your Seats
                            <Tag size={'sm'} borderRadius='full' variant='solid' colorScheme='orange' ml='10px'>
                                <TagLabel fontSize={'sm'}>{seatsLeftBadgeText}</TagLabel>
                            </Tag>                                            
                        </Button>
                    </Link>
                </Center>
            </Box>
            <Text textAlign={'center'} fontSize='4xl' fontWeight={'bold'} style={{lineHeight: 0.9}}>{showNameText}</Text>
            <Text textAlign={'center'} fontSize='3xl' >{venueText}</Text>
        </Stack>
    )
}
